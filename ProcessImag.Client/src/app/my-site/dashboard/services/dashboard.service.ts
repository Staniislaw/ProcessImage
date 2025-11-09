import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Stat {
  label: string;
  value: string | number;
  icon: string;
  color: string;
}

export interface ProcessingData {
  id: number;
  imagineId: number;
  tipProcesare: string;
  status: string;
  dataProcesare: string;
}

export interface FileData {
  id: number;
  nume: string;
  tip: string;
  utilizatorId: number;
  dataIncarcarii: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/DashBoard`;

  getStats(): Observable<Stat[]> {
    return this.http.get<Stat[]>(`${this.apiUrl}/stats`).pipe(
      catchError(err => {
        console.error('Eroare la preluarea statistici:', err);
        return of([]);
      })
    );
  }

  getProcessingData(): Observable<ProcessingData[]> {
    return this.http.get<ProcessingData[]>(`${this.apiUrl}/processing`).pipe(
      catchError(err => {
        console.error('Eroare la preluarea procesărilor:', err);
        return of([]);
      })
    );
  }

  getFilesData(): Observable<FileData[]> {
    return this.http.get<FileData[]>(`${this.apiUrl}/files`).pipe(
      catchError(err => {
        console.error('Eroare la preluarea fișierelor:', err);
        return of([]);
      })
    );
  }
}
