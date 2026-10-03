import { convertFile, downloadVideo } from './api';

describe('api', () => {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    fetchMock.mockReset();
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => vi.unstubAllGlobals());

  it('sends the file and format as multipart and uses the file name chosen by the server', async () => {
    fetchMock.mockResolvedValue(
      new Response('converted', {
        headers: {
          'Content-Disposition': "attachment; filename=foto.webp; filename*=UTF-8''foto.webp",
        },
      }),
    );

    const result = await convertFile('convert/image', new File(['x'], 'foto.png'), 'WebP');

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('http://localhost:5107/api/media/convert/image');
    expect(init?.method).toBe('POST');
    const body = init?.body as FormData;
    expect((body.get('file') as File).name).toBe('foto.png');
    expect(body.get('format')).toBe('WebP');
    expect(result.fileName).toBe('foto.webp');
    expect(await result.blob.text()).toBe('converted');
  });

  it('posts the video link as JSON', async () => {
    fetchMock.mockResolvedValue(new Response('video'));

    await downloadVideo('https://youtu.be/abc', 'MP4');

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('http://localhost:5107/api/media/download');
    expect(JSON.parse(init?.body as string)).toEqual({
      url: 'https://youtu.be/abc',
      format: 'MP4',
    });
  });

  it('decodes accented file names', async () => {
    fetchMock.mockResolvedValue(
      new Response('x', {
        headers: { 'Content-Disposition': "attachment; filename*=UTF-8''v%C3%ADdeo.mp4" },
      }),
    );

    expect((await downloadVideo('https://a.com', 'MP4')).fileName).toBe('vídeo.mp4');
  });

  it('uses the "detail" message from an error response', async () => {
    fetchMock.mockResolvedValue(Response.json({ detail: 'Formato inválido.' }, { status: 400 }));

    await expect(convertFile('convert/image', new File(['x'], 'a.png'), 'PNG')).rejects.toThrow(
      'Formato inválido.',
    );
  });

  it('uses a generic message when the error response is not JSON', async () => {
    fetchMock.mockResolvedValue(new Response('boom', { status: 500 }));

    await expect(downloadVideo('https://a.com', 'MP4')).rejects.toThrow(
      'Não foi possível concluir a operação.',
    );
  });

  it('reports a connection error when the server is unreachable', async () => {
    fetchMock.mockRejectedValue(new TypeError('Failed to fetch'));

    await expect(downloadVideo('https://a.com', 'MP4')).rejects.toThrow(
      'Não foi possível conectar ao servidor.',
    );
  });
});
