export interface ErrorBody {
  request_id: null;
  error: {
    code: string | null;
    title: string;
    description: string | null;
  };
}

export function errorResponse(
  title: string,
  description: string | null = null,
  code: string | null = null,
): ErrorBody {
  return {
    request_id: null,
    error: { code, title, description },
  };
}
