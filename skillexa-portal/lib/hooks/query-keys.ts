export const queryKeys = {
  hello: ["hello"] as const,
  jobs: {
    all: ["jobs"] as const,
    detail: (jobId: number) => ["jobs", jobId] as const,
  },
  usage: {
    current: ["usage"] as const,
  },
} as const;
