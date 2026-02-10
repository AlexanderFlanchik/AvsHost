import type { Channel } from "amqplib";
import * as amqp from "amqplib";
import { randomUUIDv7 } from "bun";
import type { Container } from "../common/DependcyInjection";
import type { CustomRouteHandlerChanged } from "../dtos/CustomRouteHandlerChanged";
import CustomRouteHandlerChangedConsumer from "./CustomRouteHandlerChangedConsumer";
import type { CustomRouteHandlersDeleted } from "../dtos/CustomRouteHandlersDeleted";
import CustomRouteHandlersDeletedConsumer from "./CustomRouteHandlersDeletedConsumer";

const rabbitMqConnectionString = process.env.RABBITMQ_CONNECTION_STRING || "rabbitmq://localhost:55720";
const handlerChangeExchange = "Avs.StaticSiteHosting.Shared.Contracts.CustomRouteHandlerChanged";
const handlerDeleteExchange = "Avs.StaticSiteHosting.Shared.Contracts.CustomRouteHandlersDeleted";

const hostId = randomUUIDv7();
const handlerChangeQueue = `chrc-${hostId}`;
const handlerDeleteQueue = `chrd-${hostId}`;

let connection: amqp.ChannelModel | null = null;
let channel: Channel | null = null;

const startMessaging = async (container: Container) => {
    connection = await amqp.connect(rabbitMqConnectionString);
    channel = await connection.createChannel();
        
    await channel.assertExchange(handlerChangeExchange, "fanout", { durable: false, autoDelete: true });
    await channel.assertExchange(handlerDeleteExchange, "fanout", { durable: false, autoDelete: true });

    await channel.assertQueue(handlerChangeQueue, { exclusive: true });
    await channel.assertQueue(handlerDeleteQueue, { exclusive: true });

    await channel.bindQueue(handlerChangeQueue, handlerChangeExchange, "");
    await channel.bindQueue(handlerDeleteQueue, handlerDeleteExchange, "");

    channel.consume(handlerChangeQueue, (msg) => {
        if (!msg) {
            return;
        }

        try {
            const content = msg.content.toString();
            const event = JSON.parse(content) as CustomRouteHandlerChanged;
            const hanlder = container.get<CustomRouteHandlerChangedConsumer>(CustomRouteHandlerChangedConsumer);
        
            hanlder.onHandlerChanged(event);
            channel?.ack(msg);
        } catch (error) {
            console.error("Error processing handler change message:", error);
            channel?.nack(msg, false, true);
        }
    });

    channel.consume(handlerDeleteQueue, (msg) => {
        if (!msg) {
            return;
        }

        const content = msg.content.toString();
        const event = JSON.parse(content) as CustomRouteHandlersDeleted;
        const hanlder = container.get<CustomRouteHandlersDeletedConsumer>(CustomRouteHandlersDeletedConsumer);
        
        try {
            hanlder.onHandlersDeleted(event);
            console.log("Received handler delete message:", content);
            channel?.ack(msg);
        } catch (error) {
            console.error("Error processing handler delete message:", error);
            channel?.nack(msg, false, true);
        }
    });

    console.log("Messaging initialized with RabbitMQ at", rabbitMqConnectionString);
};

const stopMessaging = async () => {
    try {
        if (channel) {
            await channel.close();
        }
    
        if (connection) {
            await connection.close();
        }

        console.log("Messaging connection closed");    
    } catch (error) {
        console.error("Error stopping messaging:", error);
    }
};

export { startMessaging, stopMessaging };