import express from "express";
import pinoHttp from "pino-http";
const pinoHttpMiddleware = pinoHttp.default ?? pinoHttp;
import { logger } from "./utils/logger.js";
import { auth } from "./middleware/auth.js";
import { delay } from "./middleware/delay.js";
import { errorSimulation } from "./middleware/errorSimulation.js";
import { jobsRouter } from "./routes/jobs.js";

const PORT = Number(process.env["PORT"] ?? "3100");

const app = express();

// --- Global middleware ---
app.use(pinoHttpMiddleware({ logger }));
app.use(express.json());

// --- Health endpoint (no auth) ---
app.get("/health", (_req, res) => {
  res.json({ status: "healthy" });
});

// --- Protected routes ---
app.use(auth);
app.use(delay);
app.use(errorSimulation);
app.use(jobsRouter);

// --- Start server ---
app.listen(PORT, () => {
  logger.info({ port: PORT }, "mock-theirstack listening");
});
