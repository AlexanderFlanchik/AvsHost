FROM oven/bun:1.0

WORKDIR /app

COPY package.json ./
RUN bun install

COPY . .

EXPOSE 3032

CMD ["bun", "run", "start"]