import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DashboardService, FileData, ProcessingData, Stat } from './services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  isLoading = false;
  stats: Stat[] = [];
  processingData: ProcessingData[] = [];
  filesData: FileData[] = [];
  processingColumns: string[] = ['id', 'imagineId', 'tipProcesare', 'status', 'dataProcesare'];
  filesColumns: string[] = ['id', 'nume', 'tip', 'utilizatorId', 'dataIncarcarii'];

  constructor(private dashboardService: DashboardService) { }

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.dashboardService.getStats().subscribe({
      next: stats => this.stats = stats,
      error: err => {
        console.error('Eroare la preluarea statistici', err);
        this.stats = [];
      }
    });

    this.dashboardService.getProcessingData().subscribe({
      next: processing => this.processingData = processing,
      error: err => {
        console.error('Eroare la preluarea procesărilor', err);
        this.processingData = [];
      }
    });
    this.dashboardService.getFilesData().subscribe({
      next: files => this.filesData = files,
      error: err => {
        console.error('Eroare la preluarea fișierelor', err);
        this.filesData = [];
      },
      complete: () => this.isLoading = false
    });
  }

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  getStatusIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'success': 'check_circle',
      'failed': 'error',
      'pending': 'schedule'
    };
    return icons[status.toLowerCase()] || 'info';
  }

  getStatusChipClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'success': 'chip-success',
      'failed': 'chip-failed',
      'pending': 'chip-pending'
    };
    return statusMap[status.toLowerCase()] || 'chip-pending';
  }
}
