import { Component, signal } from '@angular/core';
import { convertFile, downloadVideo } from './api';

interface FunctionOption {
  id: string;
  label: string;
  /** Rota do backend que executa a função. */
  endpoint: string;
  /** `url`: caixa para colar o link. `file`: área de upload. */
  input: 'url' | 'file';
  /** Tipos de arquivo aceitos no upload. */
  accept?: string;
  /** Texto de ajuda mostrado na área de upload. */
  hint?: string;
  formats: string[];
}

const FUNCTIONS: FunctionOption[] = [
  {
    id: 'download',
    label: 'Download de vídeo',
    endpoint: 'download',
    input: 'url',
    formats: ['MP4', 'WebM'],
  },
  {
    id: 'video-mp3',
    label: 'Vídeo → MP3',
    endpoint: 'convert/video',
    input: 'file',
    accept: 'video/mp4',
    hint: 'Vídeo MP4',
    formats: ['MP3'],
  },
  {
    id: 'video-gif',
    label: 'Vídeo → GIF',
    endpoint: 'convert/video',
    input: 'file',
    accept: 'video/mp4',
    hint: 'Vídeo MP4',
    formats: ['GIF'],
  },
  {
    id: 'image',
    label: 'Converter imagem',
    endpoint: 'convert/image',
    input: 'file',
    accept: 'image/*',
    hint: 'JPG, PNG, WebP e outros',
    formats: ['PNG', 'JPG', 'WebP', 'GIF', 'BMP'],
  },
];

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {
  protected readonly functions = FUNCTIONS;

  protected readonly selected = signal(FUNCTIONS[0]);
  protected readonly format = signal(FUNCTIONS[0].formats[0]);
  protected readonly url = signal('');
  protected readonly file = signal<File | null>(null);
  protected readonly dragging = signal(false);
  protected readonly loading = signal(false);
  protected readonly message = signal<{ text: string; isError: boolean } | null>(null);

  protected selectFunction(id: string): void {
    const option = FUNCTIONS.find((f) => f.id === id)!;
    this.selected.set(option);
    this.format.set(option.formats[0]);
    this.url.set('');
    this.file.set(null);
    this.message.set(null);
  }

  protected selectFile(file: File | undefined): void {
    this.file.set(file ?? null);
    this.message.set(null);
  }

  protected onFileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectFile(input.files?.[0]);
    input.value = ''; // permite escolher o mesmo arquivo de novo (senão o navegador não avisa a mudança)
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault(); // sem isso o navegador abre o arquivo em vez de entregá-lo à página
    this.dragging.set(false);
    this.selectFile(event.dataTransfer?.files[0]);
  }

  protected canStart(): boolean {
    if (this.loading()) return false;
    return this.selected().input === 'url' ? this.url().trim() !== '' : this.file() !== null;
  }

  protected async start(): Promise<void> {
    const option = this.selected();
    this.message.set(null);
    this.loading.set(true);

    try {
      const result =
        option.input === 'url'
          ? await downloadVideo(this.url().trim(), this.format())
          : await convertFile(option.endpoint, this.file()!, this.format());

      saveFile(result.blob, result.fileName);
      this.message.set({ text: `Pronto! "${result.fileName}" foi baixado.`, isError: false });
    } catch (error) {
      this.message.set({ text: (error as Error).message, isError: true });
    } finally {
      this.loading.set(false);
    }
  }
}

/** Faz o navegador baixar o arquivo, usando um link temporário. */
function saveFile(blob: Blob, fileName: string): void {
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(link.href);
}
