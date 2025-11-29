import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
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

  getProcessingData(pageIndex: number = 0, pageSize: number = 10): Observable<{ processing: ProcessingData[], total: number }> {
    const params = new HttpParams()
      .set('skip', (pageIndex * pageSize).toString())
      .set('take', pageSize.toString());
    return this.http.get<{ processing: ProcessingData[], total: number }>(`${this.apiUrl}/processing`, { params }).pipe(
      catchError(err => {
        console.error('Eroare la preluarea fișierelor:', err);
        return of({ processing: [], total: 0 });
      })
    );
  }

  getFilesData(pageIndex: number = 0, pageSize: number = 10): Observable<{ files: FileData[], total: number }> {
    const params = new HttpParams()
      .set('skip', (pageIndex * pageSize).toString())
      .set('take', pageSize.toString());

    return this.http.get<{ files: FileData[], total: number }>(`${this.apiUrl}/files`, { params }).pipe(
      catchError(err => {
        console.error('Eroare la preluarea fișierelor:', err);
        return of({ files: [], total: 0 });
      })
    );
  }
  deleteProcessingData(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteProcessingData/${id}`).pipe(
      catchError(err => {
        console.error('Eroare la ștergerea datelor procesate:', err);
        return of(null);
      })
    );
  }
  deleteFile(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteFile/${id}`).pipe(
      catchError(err => {
        console.error('Eroare la ștergerea fișierului:', err);
        return of(null);
      })
    );
  }

}
