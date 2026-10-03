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
});
