# Stage 1: Build
ARG NODE_VERSION=24.14.0-alpine3.23
FROM node:${NODE_VERSION} AS build
RUN apk upgrade --no-cache && npm install -g pnpm@11.0.9

WORKDIR /app

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

COPY tsconfig.json ./
COPY src ./src
RUN pnpm run build

# Stage 2: Production
FROM node:${NODE_VERSION}
RUN apk upgrade --no-cache && npm install -g pnpm@11.0.9

WORKDIR /app

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile --prod

COPY --from=build /app/dist ./dist
COPY src/fixtures ./dist/fixtures

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

EXPOSE 3100

CMD ["node", "dist/index.js"]
