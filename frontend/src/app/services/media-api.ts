import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const API_URL = 'http://localhost:5107/api/media';

export interface DownloadableFile {
  blob: Blob;
  fileName: string;
}

@Injectable({ providedIn: 'root' })
export class MediaApi {
  private readonly http = inject(HttpClient);

  /** Envia a imagem para o backend e devolve o arquivo convertido. Lança um `Error` com mensagem amigável. */
  convertImage(file: File, format: string): Promise<DownloadableFile> {
    const body = new FormData();
    body.append('file', file);
    body.append('format', format);

    const baseName = file.name.replace(/\.[^.]+$/, '');
    return this.send('convert/image', body, `${baseName}.${format.toLowerCase()}`);
  }

  /** Envia o vídeo para o backend e devolve o áudio (MP3) ou o GIF. Lança um `Error` com mensagem amigável. */
  convertVideo(file: File, format: string): Promise<DownloadableFile> {
    const body = new FormData();
    body.append('file', file);
    body.append('format', format);

    const baseName = file.name.replace(/\.[^.]+$/, '');
    return this.send('convert/video', body, `${baseName}.${format.toLowerCase()}`);
  }

  /** Pede ao backend para baixar o vídeo do link. Lança um `Error` com mensagem amigável. */
  downloadVideo(url: string, format: string): Promise<DownloadableFile> {
    return this.send('download', { url, format }, `video.${format.toLowerCase()}`);
  }

  private async send(path: string, body: FormData | object, fallbackName: string): Promise<DownloadableFile> {
    try {
      const response = await firstValueFrom(
        this.http.post(`${API_URL}/${path}`, body, { responseType: 'blob', observe: 'response' }),
      );
      const fileName = this.fileNameFrom(response.headers.get('Content-Disposition')) ?? fallbackName;
      return { blob: response.body!, fileName };
    } catch (error) {
      throw new Error(await this.toMessage(error));
    }
  }

  /** Lê o nome do arquivo escolhido pelo servidor (`filename*=UTF-8''x.mp4` ou `filename=x.mp4`). */
  private fileNameFrom(contentDisposition: string | null): string | null {
    if (!contentDisposition) return null;

    const encoded = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition)?.[1];
    if (encoded) {
      try {
        return decodeURIComponent(encoded);
      } catch {
        // nome mal codificado: tenta o formato simples abaixo
      }
    }
    return /filename="?([^";]+)"?/i.exec(contentDisposition)?.[1] ?? null;
  }

  private async toMessage(error: unknown): Promise<string> {
    if (!(error instanceof HttpErrorResponse)) return 'Erro inesperado.';
    if (error.status === 0) return 'Não foi possível conectar ao servidor.';

    // Como pedimos `blob`, o corpo do erro (JSON com `detail`) também chega como Blob.
    if (error.error instanceof Blob) {
      try {
        const problem = JSON.parse(await error.error.text());
        if (typeof problem.detail === 'string') return problem.detail;
      } catch {
        // corpo não era JSON: cai na mensagem padrão abaixo
      }
    }
    return 'Não foi possível concluir a operação.';
  }
}
