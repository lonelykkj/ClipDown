import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MediaApi } from './media-api';

describe('MediaApi', () => {
  let api: MediaApi;
  let http: HttpTestingController;
  const url = 'http://localhost:5107/api/media/convert/image';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(MediaApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends file and format as multipart and returns the converted file', async () => {
    const promise = api.convertImage(new File(['x'], 'foto.png'), 'WebP');

    const req = http.expectOne(url);
    expect(req.request.method).toBe('POST');
    const body = req.request.body as FormData;
    expect((body.get('file') as File).name).toBe('foto.png');
    expect(body.get('format')).toBe('WebP');
    req.flush(new Blob(['converted']));

    const result = await promise;
    expect(result.fileName).toBe('foto.webp');
    expect(result.blob.size).toBeGreaterThan(0);
  });

  it('uses the "detail" message from a 400 problem response', async () => {
    const promise = api.convertImage(new File(['x'], 'a.png'), 'PNG');
    const problem = new Blob([JSON.stringify({ detail: 'Formato inválido.' })]);

    http.expectOne(url).flush(problem, { status: 400, statusText: 'Bad Request' });

    await expect(promise).rejects.toThrow('Formato inválido.');
  });

  it('reports a connection error when the server is unreachable', async () => {
    const promise = api.convertImage(new File(['x'], 'a.png'), 'PNG');

    http.expectOne(url).error(new ProgressEvent('error'));

    await expect(promise).rejects.toThrow('Não foi possível conectar ao servidor.');
  });

  it('posts the video link as JSON and uses the file name chosen by the server', async () => {
    const promise = api.downloadVideo('https://youtu.be/abc', 'MP4');

    const req = http.expectOne('http://localhost:5107/api/media/download');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ url: 'https://youtu.be/abc', format: 'MP4' });
    req.flush(new Blob(['video']), {
      headers: { 'Content-Disposition': "attachment; filename=Meu_Video.mp4; filename*=UTF-8''Meu_Video.mp4" },
    });

    const result = await promise;
    expect(result.fileName).toBe('Meu_Video.mp4');
  });

  it('falls back to a default name when the server sends none', async () => {
    const promise = api.downloadVideo('https://youtu.be/abc', 'WebM');

    http.expectOne('http://localhost:5107/api/media/download').flush(new Blob(['video']));

    expect((await promise).fileName).toBe('video.webm');
  });

  it('sends the video as multipart to the video conversion endpoint', async () => {
    const promise = api.convertVideo(new File(['x'], 'clipe.mp4'), 'MP3');

    const req = http.expectOne('http://localhost:5107/api/media/convert/video');
    expect(req.request.method).toBe('POST');
    const body = req.request.body as FormData;
    expect((body.get('file') as File).name).toBe('clipe.mp4');
    expect(body.get('format')).toBe('MP3');
    req.flush(new Blob(['audio']));

    expect((await promise).fileName).toBe('clipe.mp3');
  });
});
