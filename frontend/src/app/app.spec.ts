import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('ClipDown');
  });

  it('should show url input for download and file upload for conversions', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('input[type=url]')).toBeTruthy();

    const select = el.querySelector('select[name=function]') as HTMLSelectElement;
    select.value = 'image';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();
    expect(el.querySelector('input[type=url]')).toBeNull();
    expect(el.querySelector('input[type=file]')).toBeTruthy();
  });

  it('enables "Iniciar" only after a link is typed', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const el = fixture.nativeElement as HTMLElement;
    const button = el.querySelector('button[type=submit]') as HTMLButtonElement;
    expect(button.disabled).toBe(true);

    const input = el.querySelector('input[type=url]') as HTMLInputElement;
    input.value = 'https://youtu.be/abc';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    expect(button.disabled).toBe(false);
  });

  it('shows only the formats of the selected function', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const el = fixture.nativeElement as HTMLElement;
    const formats = () =>
      [...el.querySelectorAll('select[name=format] option')].map((o) => o.textContent?.trim());
    expect(formats()).toEqual(['MP4', 'WebM']);

    const select = el.querySelector('select[name=function]') as HTMLSelectElement;
    select.value = 'video-mp3';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();
    expect(formats()).toEqual(['MP3']);
  });
});
