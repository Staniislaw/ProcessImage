import { Component, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface FilterConfig {
  type: FilterType;
  label: string;
  icon: string;
  description: string;
  min: number;
  max: number;
  default: number;
  unit: string;
}

type FilterType = 'grayscale' | 'blur' | 'invert' | 'sepia' | 'brightness' | 'contrast' | 'saturate' | 'hue-rotate';

@Component({
  selector: 'app-image-processing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './process-image.component.html',
  styleUrl: './process-image.component.css'
})
export class ProcessImageComponent {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;

  originalImage: string | null = null;
  processedImage: string | null = null;
  filterValue: number = 100;
  isProcessing: boolean = false;
  isDragging: boolean = false;
  toastMessage: string = '';
  showToast: boolean = false;

  filters: FilterConfig[] = [
    {
      type: 'grayscale',
      label: 'Grayscale',
      icon: 'palette',
      description: 'Convert to black and white',
      min: 0,
      max: 100,
      default: 100,
      unit: '%'
    },
    {
      type: 'blur',
      label: 'Blur',
      icon: 'blur_on',
      description: 'Add blur effect',
      min: 0,
      max: 20,
      default: 5,
      unit: 'px'
    },
    {
      type: 'invert',
      label: 'Invert',
      icon: 'invert_colors',
      description: 'Invert colors',
      min: 0,
      max: 100,
      default: 100,
      unit: '%'
    },
    {
      type: 'sepia',
      label: 'Sepia',
      icon: 'auto_awesome',
      description: 'Apply vintage sepia tone',
      min: 0,
      max: 100,
      default: 100,
      unit: '%'
    },
    {
      type: 'brightness',
      label: 'Brightness',
      icon: 'wb_sunny',
      description: 'Adjust brightness',
      min: 0,
      max: 200,
      default: 150,
      unit: '%'
    },
    {
      type: 'contrast',
      label: 'Contrast',
      icon: 'contrast',
      description: 'Adjust contrast',
      min: 0,
      max: 200,
      default: 150,
      unit: '%'
    },
    {
      type: 'saturate',
      label: 'Saturation',
      icon: 'palette',
      description: 'Adjust color saturation',
      min: 0,
      max: 200,
      default: 150,
      unit: '%'
    },
    {
      type: 'hue-rotate',
      label: 'Hue Rotate',
      icon: 'palette',
      description: 'Rotate color hue',
      min: 0,
      max: 360,
      default: 180,
      unit: 'deg'
    }
  ];

  selectedFilter: FilterConfig = this.filters[0];

  handleFileSelect(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.showToastMessage('Please select an image file', 'error');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      this.originalImage = e.target?.result as string;
      this.processedImage = null;
      this.showToastMessage('Image loaded successfully!', 'success');
    };
    reader.readAsDataURL(file);
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.handleFileSelect(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(): void {
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.handleFileSelect(file);
    }
  }

  openFileDialog(): void {
    this.fileInput.nativeElement.click();
  }

  processImage(): void {
    if (!this.originalImage || !this.canvas) return;

    this.isProcessing = true;

    const img = new Image();
    img.onload = () => {
      const canvas = this.canvas.nativeElement;
      const ctx = canvas.getContext('2d')!;
      
      canvas.width = img.width;
      canvas.height = img.height;

      let filterString = '';
      switch (this.selectedFilter.type) {
        case 'grayscale':
          filterString = `grayscale(${this.filterValue}%)`;
          break;
        case 'blur':
          filterString = `blur(${this.filterValue}px)`;
          break;
        case 'invert':
          filterString = `invert(${this.filterValue}%)`;
          break;
        case 'sepia':
          filterString = `sepia(${this.filterValue}%)`;
          break;
        case 'brightness':
          filterString = `brightness(${this.filterValue}%)`;
          break;
        case 'contrast':
          filterString = `contrast(${this.filterValue}%)`;
          break;
        case 'saturate':
          filterString = `saturate(${this.filterValue}%)`;
          break;
        case 'hue-rotate':
          filterString = `hue-rotate(${this.filterValue}deg)`;
          break;
      }

      ctx.filter = filterString;
      ctx.drawImage(img, 0, 0);

      this.processedImage = canvas.toDataURL('image/png');
      this.isProcessing = false;
      this.showToastMessage('Image processed successfully!', 'success');
    };

    img.src = this.originalImage;
  }

  downloadImage(): void {
    if (!this.processedImage) return;

    const link = document.createElement('a');
    link.download = `processed-${this.selectedFilter.type}-${Date.now()}.png`;
    link.href = this.processedImage;
    link.click();
    this.showToastMessage('Image downloaded!', 'success');
  }

  resetImage(): void {
    this.originalImage = null;
    this.processedImage = null;
    this.filterValue = this.selectedFilter.default;
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
    this.showToastMessage('Reset complete', 'info');
  }

  selectFilter(filter: FilterConfig): void {
    this.selectedFilter = filter;
    this.filterValue = filter.default;
    this.processedImage = null;
  }

  get firstRowFilters(): FilterConfig[] {
    return this.filters.slice(0, 4);
  }

  get secondRowFilters(): FilterConfig[] {
    return this.filters.slice(4);
  }

  showToastMessage(message: string, type: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }
}
