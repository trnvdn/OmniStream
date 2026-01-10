# Build stage
FROM node:22-alpine AS build

WORKDIR /app

COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

COPY . .

ARG VITE_API_URL
ARG VITE_HUB_URL
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_HUB_URL=$VITE_HUB_URL

RUN yarn build

# Production stage
FROM node:22-alpine

WORKDIR /app

RUN yarn global add serve

COPY --from=build /app/dist ./dist

EXPOSE 5173

CMD ["serve", "-s", "dist", "-l", "5173"]
