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
  async convertImage(file: File, format: string): Promise<DownloadableFile> {
    const body = new FormData();
    body.append('file', file);
    body.append('format', format);

    try {
      const blob = await firstValueFrom(
        this.http.post(`${API_URL}/convert/image`, body, { responseType: 'blob' }),
      );
      const baseName = file.name.replace(/\.[^.]+$/, '');
      return { blob, fileName: `${baseName}.${format.toLowerCase()}` };
    } catch (error) {
      throw new Error(await this.toMessage(error));
    }
  }

  private async toMessage(error: unknown): Promise<string> {
    if (!(error instanceof HttpErrorResponse)) return 'Erro inesperado ao converter a imagem.';
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
    return 'Não foi possível converter a imagem.';
  }
}
