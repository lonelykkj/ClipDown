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
});
