import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProcessingType } from './process-image.component';

@Injectable({
  providedIn: 'root'
})
export class ImageProcessingService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiBaseUrl + '/ImageProcessing';

  processResize(file: File, scale: number): Observable<Blob> {
    const formData = new FormData();
    formData.append('image', file);
    formData.append('scale', scale.toString());
    return this.http.post(`${this.apiUrl}/resize`, formData, { responseType: 'blob' });
  }

  processCrop(file: File, x: number, y: number, width: number, height: number): Observable<Blob> {
    const formData = new FormData();
    formData.append('image', file);
    formData.append('x', x.toString());
    formData.append('y', y.toString());
    formData.append('width', width.toString());
    formData.append('height', height.toString());
    return this.http.post(`${this.apiUrl}/crop`, formData, { responseType: 'blob' });
  }

  processRotate(file: File, angle: number): Observable<Blob> {
    const formData = new FormData();
    formData.append('image', file);
    formData.append('angle', angle.toString());
    return this.http.post(`${this.apiUrl}/rotate`, formData, { responseType: 'blob' });
  }

  processFilter(file: File, filterType: string, intensity: number): Observable<Blob> {
    const formData = new FormData();
    formData.append('image', file);
    formData.append('filterType', filterType);
    formData.append('intensity', intensity.toString());
    return this.http.post(`${this.apiUrl}/filter`, formData, { responseType: 'blob' });
  }

  processCompress(file: File, quality: number): Observable<Blob> {
    const formData = new FormData();
    formData.append('image', file);
    formData.append('quality', quality.toString());
    return this.http.post(`${this.apiUrl}/compress`, formData, { responseType: 'blob' });
  }

  processWatermark(
    file: File,
    text: string,
    fontSize: number = 48,
    color: string = '#FFFFFF',
    position: string = 'BottomRight',
    opacity: number = 50
  ): Observable<Blob> {
    const formData = new FormData();
    formData.append('image', file);
    formData.append('text', text);
    formData.append('fontSize', fontSize.toString());
    formData.append('color', color);
    formData.append('position', position);
    formData.append('opacity', opacity.toString());
    return this.http.post(`${this.apiUrl}/watermark`, formData, { responseType: 'blob' });
  }
  processTransferColors(sourceFile: File, referenceFile: File): Observable<Blob> {
    const formData = new FormData();
    formData.append('sourceImage', sourceFile);
    formData.append('referenceImage', referenceFile);
    return this.http.post(`${this.apiUrl}/transfer-colors`, formData, { responseType: 'blob' });
  }
  checkProcessingLimit(processingTypeId: number): Observable<{ canProcess: boolean; proceseRamase?: number; message?: string }> {
    const url = `${environment.apiBaseUrl}/subscriptions/check-limit?tipProcesareId=${processingTypeId}`;
    return this.http.get<{ canProcess: boolean; proceseRamase?: number; message?: string }>(url).pipe(
      map(result => {
        return result;
      }),
      catchError(err => {
        console.error('Eroare la verificarea limitei:', err);
        return of({ canProcess: false, message: 'Eroare la verificarea limitei' });
      })
    );
  }
  getProcessingTypes(): Observable<ProcessingType[]> {
    return this.http.get<ProcessingType[]>(`${this.apiUrl}/GetProcessingTypes/`).pipe(
      catchError((err) => {
        return of([]); 
      })
    );
  }
}