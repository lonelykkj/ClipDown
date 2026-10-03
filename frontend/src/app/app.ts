import { Component, computed, inject, signal } from '@angular/core';
import { DownloadableFile, MediaApi } from './services/media-api';

type FunctionId = 'download' | 'video-mp3' | 'video-gif' | 'image';

interface FunctionOption {
  id: FunctionId;
  label: string;
  input: 'url' | 'file';
  accept?: string;
  formats: string[];
}

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly functions: FunctionOption[] = [
    {
      id: 'download',
      label: 'Download de vídeo',
      input: 'url',
      formats: ['MP4', 'WebM']
    },
    {
      id: 'video-mp3',
      label: 'Vídeo → MP3',
      input: 'file',
      accept: 'video/mp4',
      formats: ['MP3'],
    },
    {
      id: 'video-gif',
      label: 'Vídeo → GIF',
      input: 'file',
      accept: 'video/mp4',
      formats: ['GIF'],
    },
    {
      id: 'image',
      label: 'Converter imagem',
      input: 'file',
      accept: 'image/*',
      formats: ['PNG', 'JPG', 'WebP', 'GIF', 'BMP'],
    },
  ];

  protected readonly selectedId = signal<FunctionId>('download');
  protected readonly format = signal('MP4');
  protected readonly url = signal('');
  protected readonly file = signal<File | null>(null);
  protected readonly dragging = signal(false);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  private readonly mediaApi = inject(MediaApi);

  protected readonly current = computed(
    () => this.functions.find((f) => f.id === this.selectedId())!,
  );

  protected readonly canStart = computed(
    () =>
      !this.loading() &&
      (this.current().input === 'url' ? this.url().trim().length > 0 : this.file() !== null),
  );

  protected onFunctionChange(id: string): void {
    this.selectedId.set(id as FunctionId);
    this.format.set(this.current().formats[0]);
    this.url.set('');
    this.file.set(null);
    this.clearMessages();
  }

  protected onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.file.set(input.files?.[0] ?? null);
    this.clearMessages();
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    this.file.set(event.dataTransfer?.files?.[0] ?? null);
    this.clearMessages();
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(true);
  }

  protected async start(): Promise<void> {
    this.clearMessages();

    const task = this.buildTask();
    if (!task) {
      this.error.set('Esta função ainda não está disponível.');
      return;
    }

    this.loading.set(true);
    try {
      const result = await task();
      this.saveFile(result.blob, result.fileName);
      this.success.set(`Pronto! "${result.fileName}" foi baixado.`);
    } catch (e) {
      this.error.set(e instanceof Error ? e.message : 'Erro inesperado.');
    } finally {
      this.loading.set(false);
    }
  }

  /** Devolve a chamada ao backend da função escolhida, ou `null` se ainda não estiver integrada. */
  private buildTask(): (() => Promise<DownloadableFile>) | null {
    const format = this.format();

    switch (this.selectedId()) {
      case 'download':
        return () => this.mediaApi.downloadVideo(this.url().trim(), format);
      case 'image': {
        const file = this.file();
        return file ? () => this.mediaApi.convertImage(file, format) : null;
      }
      default:
        return null;
    }
  }

  private clearMessages(): void {
    this.error.set(null);
    this.success.set(null);
  }

  /** Dispara o download no navegador a partir de um Blob. */
  private saveFile(blob: Blob, fileName: string): void {
    const objectUrl = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = objectUrl;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(objectUrl);
  }
}
