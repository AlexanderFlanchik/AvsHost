import { MongoClient } from "mongodb";

const mongoUrl = process.env.MONGODB_CONNECTION_STRING || "mongodb://localhost:27017";
const client = new MongoClient(mongoUrl);

export default client;
