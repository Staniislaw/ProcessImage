import { Component, ElementRef, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface ProcessingType {
  id: number;
  nume: string;
  icon?: string;
  description?: string;
}

interface ProcessingConfig {
  id: number;
  type: string;
  label: string;
  icon: string;
  description: string;
  hasIntensity: boolean;
  min: number;
  max: number;
  default: number;
  unit: string;
  customFields: CustomField[];
}

interface CustomField {
  name: string;
  label: string;
  type: 'text' | 'number' | 'color' | 'select';
  placeholder: string;
  options: string[];
  default: any;
  min: number;
  max: number;
}

@Component({
  selector: 'app-image-processing',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './process-image.component.html',
  styleUrl: './process-image.component.css'
})
export class ProcessImageComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;

  originalImage: string = '';
  processedImage: string = '';
  filterValue: number = 100;
  isProcessing: boolean = false;
  isDragging: boolean = false;
  toastMessage: string = '';
  showToast: boolean = false;
  isLoadingTypes: boolean = false;

  // Custom field values
  customFieldValues: { [key: string]: any } = {};

  // Mappings pentru icoane și descrieri
  private typeConfig: { [key: string]: { icon: string; description: string; config: Partial<ProcessingConfig> } } = {
    'Resize': {
      icon: 'photo_size_select_large',
      description: 'Redimensionează imaginea',
      config: {
        hasIntensity: true,
        min: 10,
        max: 200,
        default: 100,
        unit: '%'
      }
    },
    'Crop': {
      icon: 'crop',
      description: 'Decupează imaginea',
      config: {
        hasIntensity: false,
        customFields: [
          { name: 'x', label: 'Poziție X', type: 'number', default: 0, min: 0, max: 5000, placeholder: '', options: [] },
          { name: 'y', label: 'Poziție Y', type: 'number', default: 0, min: 0, max: 5000, placeholder: '', options: [] },
          { name: 'width', label: 'Lățime', type: 'number', default: 300, min: 1, max: 5000, placeholder: '', options: [] },
          { name: 'height', label: 'Înălțime', type: 'number', default: 300, min: 1, max: 5000, placeholder: '', options: [] }
        ]
      }
    },
    'Filter': {
      icon: 'auto_fix_high',
      description: 'Aplică filtre creative',
      config: {
        hasIntensity: true,
        min: 0,
        max: 100,
        default: 100,
        unit: '%',
        customFields: [
          {
            name: 'filterType',
            label: 'Tip Filtru',
            type: 'select',
            options: ['Grayscale', 'Blur', 'Sepia', 'Invert', 'Brightness', 'Contrast'],
            default: 'Grayscale',
            placeholder: '',
            min: 0,
            max: 0
          }
        ]
      }
    },
    'Compress': {
      icon: 'compress',
      description: 'Comprimă imaginea',
      config: {
        hasIntensity: true,
        min: 1,
        max: 100,
        default: 80,
        unit: '%'
      }
    },
    'Rotate': {
      icon: 'rotate_right',
      description: 'Rotește imaginea',
      config: {
        hasIntensity: true,
        min: 0,
        max: 360,
        default: 90,
        unit: '°'
      }
    },
    'Watermark': {
      icon: 'text_fields',
      description: 'Adaugă watermark text',
      config: {
        hasIntensity: false,
        customFields: [
          { name: 'text', label: 'Text Watermark', type: 'text', placeholder: 'Introdu text watermark', default: 'Watermark', min: 0, max: 0, options: [] },
          { name: 'fontSize', label: 'Mărime Font', type: 'number', default: 48, min: 12, max: 200, placeholder: '', options: [] },
          { name: 'color', label: 'Culoare Text', type: 'color', default: '#ffffff', placeholder: '', min: 0, max: 0, options: [] },
          { name: 'opacity', label: 'Opacitate', type: 'number', default: 50, min: 0, max: 100, placeholder: '', options: [] },
          {
            name: 'position',
            label: 'Poziție',
            type: 'select',
            options: ['Stânga Sus', 'Centru Sus', 'Dreapta Sus', 'Centru', 'Stânga Jos', 'Centru Jos', 'Dreapta Jos'],
            default: 'Dreapta Jos',
            placeholder: '',
            min: 0,
            max: 0
          }
        ]
      }
    }
  };

  // Mapping pentru traduceri nume butoane
  private nameTranslations: { [key: string]: string } = {
    'Resize': 'Redimensionare',
    'Crop': 'Decupare',
    'Filter': 'Filtre',
    'Compress': 'Comprimare',
    'Rotate': 'Rotire',
    'Watermark': 'Watermark'
  };

  processingTypes: ProcessingType[] = [];
  processingConfigs: ProcessingConfig[] = [];
  selectedConfig: ProcessingConfig = {
    id: 0,
    type: 'resize',
    label: 'Redimensionare',
    icon: 'photo_size_select_large',
    description: 'Redimensionează imaginea',
    hasIntensity: true,
    min: 10,
    max: 200,
    default: 100,
    unit: '%',
    customFields: []
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadProcessingTypes();
  }

  loadProcessingTypes(): void {
    this.isLoadingTypes = true;
    
    // Înlocuiește cu URL-ul tău de backend
    this.http.get<ProcessingType[]>('YOUR_BACKEND_URL/api/processing-types')
      .subscribe({
        next: (types) => {
          this.processingTypes = types;
          this.createProcessingConfigs();
          if (this.processingConfigs.length > 0) {
            this.selectConfig(this.processingConfigs[0]);
          }
          this.isLoadingTypes = false;
        },
        error: (error) => {
          console.error('Error loading processing types:', error);
          this.showToastMessage('Nu s-au putut încărca tipurile de procesare', 'error');
          this.isLoadingTypes = false;
          // Fallback la date mock pentru development
          this.loadMockData();
        }
      });
  }

  loadMockData(): void {
    // Date mock pentru development/testing
    this.processingTypes = [
      { id: 1, nume: 'Resize' },
      { id: 2, nume: 'Crop' },
      { id: 3, nume: 'Filter' },
      { id: 4, nume: 'Compress' },
      { id: 5, nume: 'Rotate' },
      { id: 6, nume: 'Watermark' }
    ];
    this.createProcessingConfigs();
    if (this.processingConfigs.length > 0) {
      this.selectConfig(this.processingConfigs[0]);
    }
  }

  createProcessingConfigs(): void {
    this.processingConfigs = this.processingTypes.map(type => {
      const config = this.typeConfig[type.nume] || {
        icon: 'image',
        description: type.nume,
        config: { hasIntensity: true, min: 0, max: 100, default: 100, unit: '%' }
      };

      // Traduce numele
      const translatedName = this.nameTranslations[type.nume] || type.nume;

      return {
        id: type.id,
        type: type.nume.toLowerCase(),
        label: translatedName,
        icon: config.icon,
        description: config.description,
        hasIntensity: config.config.hasIntensity ?? true,
        min: config.config.min ?? 0,
        max: config.config.max ?? 100,
        default: config.config.default ?? 100,
        unit: config.config.unit ?? '%',
        customFields: config.config.customFields || []
      };
    });
  }

  handleFileSelect(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.showToastMessage('Te rugăm selectează un fișier imagine', 'error');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      this.originalImage = (e.target?.result as string) || '';
      this.processedImage = '';
      this.showToastMessage('Imagine încărcată cu succes!', 'success');
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
    if (!this.originalImage || !this.canvas || !this.selectedConfig) return;

    this.isProcessing = true;

    const img = new Image();
    img.onload = () => {
      const canvas = this.canvas.nativeElement;
      const ctx = canvas.getContext('2d')!;
      
      // Reset canvas
      canvas.width = img.width;
      canvas.height = img.height;
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Apply processing based on type
      switch (this.selectedConfig.type) {
        case 'resize':
          this.applyResize(ctx, img);
          break;
        case 'crop':
          this.applyCrop(ctx, img);
          break;
        case 'filter':
          this.applyFilter(ctx, img);
          break;
        case 'compress':
          this.applyCompress(ctx, img);
          break;
        case 'rotate':
          this.applyRotate(ctx, img);
          break;
        case 'watermark':
          this.applyWatermark(ctx, img);
          break;
        default:
          ctx.drawImage(img, 0, 0);
      }

      this.processedImage = canvas.toDataURL('image/png');
      this.isProcessing = false;
      this.showToastMessage('Imagine procesată cu succes!', 'success');
    };

    img.src = this.originalImage;
  }

  applyResize(ctx: CanvasRenderingContext2D, img: HTMLImageElement): void {
    const scale = this.filterValue / 100;
    const canvas = ctx.canvas;
    canvas.width = img.width * scale;
    canvas.height = img.height * scale;
    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
  }

  applyCrop(ctx: CanvasRenderingContext2D, img: HTMLImageElement): void {
    const x = this.customFieldValues['x'] || 0;
    const y = this.customFieldValues['y'] || 0;
    const width = this.customFieldValues['width'] || 300;
    const height = this.customFieldValues['height'] || 300;
    
    const canvas = ctx.canvas;
    canvas.width = width;
    canvas.height = height;
    ctx.drawImage(img, x, y, width, height, 0, 0, width, height);
  }

  applyFilter(ctx: CanvasRenderingContext2D, img: HTMLImageElement): void {
    const filterType = (this.customFieldValues['filterType'] || 'Grayscale').toLowerCase();
    const intensity = this.filterValue;

    let filterString = '';
    switch (filterType) {
      case 'grayscale':
        filterString = `grayscale(${intensity}%)`;
        break;
      case 'blur':
        filterString = `blur(${intensity / 10}px)`;
        break;
      case 'sepia':
        filterString = `sepia(${intensity}%)`;
        break;
      case 'invert':
        filterString = `invert(${intensity}%)`;
        break;
      case 'brightness':
        filterString = `brightness(${intensity}%)`;
        break;
      case 'contrast':
        filterString = `contrast(${intensity}%)`;
        break;
    }

    ctx.filter = filterString;
    ctx.drawImage(img, 0, 0);
  }

  applyCompress(ctx: CanvasRenderingContext2D, img: HTMLImageElement): void {
    ctx.drawImage(img, 0, 0);
    // Note: compression will be applied in toDataURL with quality parameter
  }

  applyRotate(ctx: CanvasRenderingContext2D, img: HTMLImageElement): void {
    const canvas = ctx.canvas;
    const angle = (this.filterValue * Math.PI) / 180;
    
    // Adjust canvas size for rotation
    const sin = Math.abs(Math.sin(angle));
    const cos = Math.abs(Math.cos(angle));
    canvas.width = img.width * cos + img.height * sin;
    canvas.height = img.width * sin + img.height * cos;
    
    ctx.translate(canvas.width / 2, canvas.height / 2);
    ctx.rotate(angle);
    ctx.drawImage(img, -img.width / 2, -img.height / 2);
  }

  applyWatermark(ctx: CanvasRenderingContext2D, img: HTMLImageElement): void {
    ctx.drawImage(img, 0, 0);
    
    const text = this.customFieldValues['text'] || 'Watermark';
    const fontSize = this.customFieldValues['fontSize'] || 48;
    const color = this.customFieldValues['color'] || '#ffffff';
    const opacity = (this.customFieldValues['opacity'] || 50) / 100;
    const position = this.customFieldValues['position'] || 'Dreapta Jos';
    
    ctx.font = `bold ${fontSize}px Arial`;
    ctx.fillStyle = color;
    ctx.globalAlpha = opacity;
    
    const textMetrics = ctx.measureText(text);
    const textWidth = textMetrics.width;
    const textHeight = fontSize;
    
    let x = 0, y = 0;
    const padding = 20;
    
    switch (position) {
      case 'Stânga Sus':
        x = padding;
        y = textHeight + padding;
        break;
      case 'Centru Sus':
        x = (ctx.canvas.width - textWidth) / 2;
        y = textHeight + padding;
        break;
      case 'Dreapta Sus':
        x = ctx.canvas.width - textWidth - padding;
        y = textHeight + padding;
        break;
      case 'Centru':
        x = (ctx.canvas.width - textWidth) / 2;
        y = (ctx.canvas.height + textHeight) / 2;
        break;
      case 'Stânga Jos':
        x = padding;
        y = ctx.canvas.height - padding;
        break;
      case 'Centru Jos':
        x = (ctx.canvas.width - textWidth) / 2;
        y = ctx.canvas.height - padding;
        break;
      case 'Dreapta Jos':
        x = ctx.canvas.width - textWidth - padding;
        y = ctx.canvas.height - padding;
        break;
    }
    
    ctx.fillText(text, x, y);
    ctx.globalAlpha = 1;
  }

  downloadImage(): void {
    if (!this.processedImage || !this.selectedConfig) return;

    const link = document.createElement('a');
    link.download = `procesata-${this.selectedConfig.type}-${Date.now()}.png`;
    link.href = this.processedImage;
    link.click();
    this.showToastMessage('Imagine descărcată!', 'success');
  }

  resetImage(): void {
    this.originalImage = '';
    this.processedImage = '';
    if (this.selectedConfig) {
      this.filterValue = this.selectedConfig.default;
      this.initializeCustomFields();
    }
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
    this.showToastMessage('Resetare completă', 'info');
  }

  selectConfig(config: ProcessingConfig): void {
    this.selectedConfig = config;
    this.filterValue = config.default;
    this.processedImage = '';
    this.initializeCustomFields();
  }

  initializeCustomFields(): void {
    this.customFieldValues = {};
    if (this.selectedConfig.customFields && this.selectedConfig.customFields.length > 0) {
      this.selectedConfig.customFields.forEach(field => {
        this.customFieldValues[field.name] = field.default;
      });
    }
  }

  get firstRowConfigs(): ProcessingConfig[] {
    return this.processingConfigs.slice(0, 3);
  }

  get secondRowConfigs(): ProcessingConfig[] {
    return this.processingConfigs.slice(3);
  }

  showToastMessage(message: string, type: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }
}