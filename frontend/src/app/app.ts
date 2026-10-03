import { Component, computed, signal } from '@angular/core';

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

  protected readonly current = computed(
    () => this.functions.find((f) => f.id === this.selectedId())!,
  );

  protected readonly canStart = computed(() =>
    this.current().input === 'url' ? this.url().trim().length > 0 : this.file() !== null,
  );

  protected onFunctionChange(id: string): void {
    this.selectedId.set(id as FunctionId);
    this.format.set(this.current().formats[0]);
    this.url.set('');
    this.file.set(null);
  }

  protected onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.file.set(input.files?.[0] ?? null);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    this.file.set(event.dataTransfer?.files?.[0] ?? null);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(true);
  }

  protected start(): void {
    // Integração com o backend será feita depois.
  }
}
