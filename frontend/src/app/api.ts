const API_URL = 'http://localhost:5107/api/media';

export interface DownloadedFile {
  blob: Blob;
  fileName: string;
}

/** Envia um arquivo (imagem ou vídeo) para ser convertido no formato escolhido. */
export function convertFile(endpoint: string, file: File, format: string): Promise<DownloadedFile> {
  const body = new FormData();
  body.append('file', file);
  body.append('format', format);
  return post(endpoint, { body });
}

/** Pede ao backend para baixar o vídeo do link. */
export function downloadVideo(url: string, format: string): Promise<DownloadedFile> {
  return post('download', {
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ url, format }),
  });
}

/** Faz o POST e devolve o arquivo da resposta. Em caso de erro, lança um `Error` com a mensagem para o usuário. */
async function post(endpoint: string, init: RequestInit): Promise<DownloadedFile> {
  let response: Response;
  try {
    response = await fetch(`${API_URL}/${endpoint}`, { method: 'POST', ...init });
  } catch {
    throw new Error('Não foi possível conectar ao servidor.');
  }

  if (!response.ok) {
    // O backend responde os erros como JSON: { "detail": "mensagem" }.
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.detail ?? 'Não foi possível concluir a operação.');
  }

  return {
    blob: await response.blob(),
    fileName: fileNameFrom(response.headers.get('Content-Disposition')),
  };
}

/** O backend envia o nome do arquivo no cabeçalho como `filename*=UTF-8''nome.ext`. */
function fileNameFrom(contentDisposition: string | null): string {
  const encoded = contentDisposition?.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
  return encoded ? decodeURIComponent(encoded) : 'download';
}
