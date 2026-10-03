# ClipDown

![Angular](https://img.shields.io/badge/Angular-21-DD0031?style=flat&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-9-512BD4?style=flat&logo=dotnet&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-4-06B6D4?style=flat&logo=tailwindcss&logoColor=white)
![FFmpeg](https://img.shields.io/badge/FFmpeg-007808?style=flat&logo=ffmpeg&logoColor=white)
![yt-dlp](https://img.shields.io/badge/yt--dlp-9B59B6?style=flat)

> Ferramenta web de uso pessoal para conversão de imagens, conversão de vídeos (MP4 → MP3/GIF) e download de vídeos por URL — sem anúncios, sem pop-ups e sem sites maliciosos.

## Sobre

O **ClipDown** é um projeto de **uso pessoal** criado para eliminar a dependência de sites de terceiros — repletos de anúncios e, muitas vezes, maliciosos — em tarefas simples de manipulação de mídia. Tudo roda localmente, com uma interface limpa e apenas o necessário.

### Funcionalidades

| Função | Entrada | Saída |
|---|---|---|
| **Conversão de imagens** | Arquivo de imagem (JPG, JPEG, PNG, WebP, entre outras extensões) | Imagem no formato escolhido |
| **Conversão em áudio** | Vídeo MP4 | MP3 |
| **Conversão em GIF** | Vídeo MP4 | GIF |
| **Download por URL** | Link de vídeo (YouTube, Instagram, TikTok, Twitter/X etc.) | Arquivo de vídeo baixado |

### Roadmap

**MVP**

- [ ] Download de vídeo por URL
- [ ] Conversão MP4 → MP3
- [ ] Conversão MP4 → GIF
- [ ] Conversão de imagens entre formatos

**Futuro**

- [ ] **Upscaling de imagens** — melhorar qualidade/resolução via IA (candidatos: Real-ESRGAN via ONNX Runtime, ou API externa)
- [ ] Empacotar dependências (FFmpeg, yt-dlp) em imagem Docker

## Interface

A interface será propositalmente minimalista:

```
┌─────────────────────────────────────┐
│                                     │
│              ClipDown               │
│                                     │
│   ┌─────────────────────────────┐   │
│   │ Cole o link aqui...         │   │
│   └─────────────────────────────┘   │
│                                     │
│   [ Função ▾ ]    [ Formato ▾ ]     │
│                                     │
│              [ Iniciar ]            │
│                                     │
└─────────────────────────────────────┘
```

- O nome **ClipDown** centralizado no meio da tela
- Logo abaixo, uma caixa de busca para **colar o link** do vídeo
- **Selects** para escolher a função (ex.: *Download de vídeo*, *Vídeo → MP3*, *Vídeo → GIF*, *Converter imagem*) e o formato de saída
- Nas funções de conversão (que partem de arquivos locais), a caixa de link dá lugar a um **upload de arquivo**
- Ao final, o arquivo processado é oferecido como **download direto**

## Stack

### Principal

| Camada | Tecnologia | Papel |
|---|---|---|
| Frontend | **Angular 21** | SPA de tela única com a caixa de busca e os seletores. Estilização com **Tailwind CSS 4** |
| Backend | **.NET 9 (ASP.NET Core Web API)** | Conversões, download por URL e orquestração das ferramentas externas |

### Complementar (recomendadas)

| Ferramenta | Uso | Motivo |
|---|---|---|
| [yt-dlp](https://github.com/yt-dlp/yt-dlp) | Download de vídeos por URL | Ferramenta de referência: suporta YouTube, Instagram, TikTok, Twitter/X e centenas de outros sites, com atualização constante conforme as plataformas mudam. Invocada pelo backend via linha de comando |
| [FFmpeg](https://ffmpeg.org/) | Conversão MP4 → MP3 / GIF | Padrão de mercado em manipulação de áudio/vídeo, incluindo geração de GIF com boa qualidade (via paleta de cores) |
| [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore) | Wrapper .NET para o FFmpeg | Permite invocar o FFmpeg a partir do backend sem montar comandos manualmente |
| [ImageSharp](https://github.com/SixLabors/ImageSharp) | Conversão de imagens | Biblioteca .NET multiplataforma para carregar/salvar JPG, PNG, WebP, GIF, BMP etc. (o `System.Drawing` é limitado ao Windows). Gratuita para uso pessoal |

> **Nota:** yt-dlp e FFmpeg são binários externos — precisam estar instalados no ambiente (e no `PATH`), ou embutidos via Docker no futuro.

## API prevista

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/media/download` | Recebe uma URL e retorna o vídeo baixado |
| `POST` | `/api/media/convert/video` | Recebe um vídeo e o converte (MP3, GIF etc.) |
| `POST` | `/api/media/convert/image` | Recebe uma imagem e a converte para o formato escolhido |

Todas as rotas retornam o arquivo processado, que o frontend oferece como download.

## Como rodar (desenvolvimento)

Pré-requisitos:

- [Node.js](https://nodejs.org) + npm
- [SDK do .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- [FFmpeg](https://ffmpeg.org/download.html) instalado e no `PATH`
- [yt-dlp](https://github.com/yt-dlp/yt-dlp/wiki/Installation) instalado e no `PATH`

```bash
# Terminal 1 — backend (ASP.NET Core)
dotnet run --project backend/ClipDown.Api

# Terminal 2 — frontend (Angular)
cd frontend
npm start        # ng serve → http://localhost:4200
```

## Estrutura do projeto

```
ClipDown/
├── .gitignore
├── README.md
├── backend/                 # API .NET (a ser criada)
└── frontend/                 # Aplicação Angular
    ├── src/
    ├── angular.json
    └── package.json
```

## Notas

- Projeto de **uso pessoal**, sem fins comerciais.
- Ao baixar/converter conteúdo de plataformas de terceiros, respeite os **termos de serviço** das plataformas e os **direitos autorais** dos conteúdos.
