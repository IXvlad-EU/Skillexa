# Dependencies stage
ARG NODE_VERSION=24.13.0-slim
FROM node:${NODE_VERSION} AS dependencies
WORKDIR /app

# Copy package files and install dependencies
COPY package.json pnpm-lock.yaml* pnpm-workspace.yaml* ./
RUN --mount=type=cache,target=/root/.local/share/pnpm/store \
  corepack enable pnpm && pnpm install --frozen-lockfile

# Build stage
FROM node:${NODE_VERSION} AS builder
WORKDIR /app

# Copy dependencies and source code
COPY --from=dependencies /app/node_modules ./node_modules
COPY . .

ENV NODE_ENV=production

# Build Next.js application
RUN --mount=type=cache,target=/app/.next/cache \
  corepack enable pnpm && pnpm build

# Runtime stage
FROM node:${NODE_VERSION} AS runner

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*

WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Copy built application
COPY --from=builder --chown=node:node /app/public ./public
COPY --from=builder --chown=node:node /app/.next/standalone ./
COPY --from=builder --chown=node:node /app/.next/static ./.next/static

# Switch to non-root user
USER node

EXPOSE 3000

# Run the application
CMD ["node", "server.js"]
