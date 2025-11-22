import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DashboardService, FileData, ProcessingData, Stat } from './services/dashboard.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Observable } from 'rxjs';
import { MatSelectModule } from '@angular/material/select';
export enum DataType {
  Files = 'FilesData',
  Processing = 'ProcessingData'
}
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatSelectModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})


export class DashboardComponent implements OnInit {

  isLoading = false;
  isLoadingFiles = false;
  DataType = DataType;
  stats: Stat[] = [];
  processingData: ProcessingData[] = [];
  filesData: FileData[] = [];
  processingColumns: string[] = ['id', 'imagineId', 'tipProcesare', 'status', 'dataProcesare'];
  filesColumns: string[] = ['id', 'nume', 'tip', 'utilizatorId', 'dataIncarcarii'];


  // Pentru Files
  pageSizeFiles = 10;
  pageIndexFiles = 0;
  totalFiles: number = 0;
  pageSizeProcessing = 10;
  pageIndexProcessing = 0;
  totalProcessingData: number = 0;
  pageSizeOptions: number[] = [5, 10, 25, 50];
  length = 0;

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
    this.loadPaginatedData(DataType.Files, this.pageIndexFiles, this.pageSizeFiles);
    this.loadPaginatedData(DataType.Processing, this.pageIndexProcessing, this.pageSizeProcessing);
  }

  handlePageEvent(event: PageEvent, type: DataType): void {
    if (type === DataType.Files) {
      this.pageIndexFiles = event.pageIndex;
      this.pageSizeFiles = event.pageSize;
      this.loadPaginatedData(type, this.pageIndexFiles, this.pageSizeFiles);
    } else {
      this.pageIndexProcessing = event.pageIndex;
      this.pageSizeProcessing = event.pageSize;
      this.loadPaginatedData(type, this.pageIndexProcessing, this.pageSizeProcessing);
    }
  }


  loadPaginatedData(type: DataType, pageIndex: number, pageSize: number): void {
    this.isLoadingFiles = true;
    let apiCall: Observable<any>;
    if (type === DataType.Files) {
      apiCall = this.dashboardService.getFilesData(pageIndex, pageSize);
    } else {
      apiCall = this.dashboardService.getProcessingData(pageIndex, pageSize);
    }
    apiCall.subscribe({
      next: response => {
        if (type === DataType.Files) {
          this.filesData = response.files;
          this.totalFiles = response.total;
        } else {
          this.processingData = response.processing;
          this.totalProcessingData = response.total;
        }
        this.isLoadingFiles = false;
      },
      error: err => {
        console.error('Eroare la preluarea datelor', err);

        if (type === DataType.Files) {
          this.filesData = [];
        } else {
          this.processingData = [];
        }
        this.isLoadingFiles = false;
      }
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
