import { Component, ElementRef, ViewChild, AfterViewInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

interface CropArea {
  x: number;
  y: number;
  width: number;
  height: number;
}

@Component({
  selector: 'app-crop-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './crop-selector.component.html',
  styleUrls: ['./crop-selector.component.css']
})
export class CropSelectorComponent implements AfterViewInit {
  @ViewChild('canvas') canvasRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('cropWrapper') wrapperRef!: ElementRef<HTMLDivElement>;
  
  @Output() cropSelected = new EventEmitter<CropArea>();
  
  imageSrc: string = '';
  private ctx: CanvasRenderingContext2D | null = null;
  private img: HTMLImageElement | null = null;
  
  private isDrawing = false;
  private startX = 0;
  private startY = 0;
  
  cropArea: CropArea | null = null;
  
  // Ratio pentru scalare
  private scaleRatio: number = 1;
  
  ngAfterViewInit() {
    const canvas = this.canvasRef?.nativeElement;
    if (canvas) {
      this.ctx = canvas.getContext('2d');
    }
  }
  
  loadImage(src: string) {
    this.imageSrc = src;
    if (this.canvasRef) {
      this.ctx = this.canvasRef.nativeElement.getContext('2d');
    }
    
    this.img = new Image();
    this.img.crossOrigin = 'anonymous';
    this.img.onload = () => {
      if (this.canvasRef && this.img) {
        const canvas = this.canvasRef.nativeElement;
        if (!this.ctx) {
          this.ctx = canvas.getContext('2d');
        }
        const maxWidth = 800;
        const maxHeight = 600;
        let width = this.img.width;
        let height = this.img.height;
        if (width > maxWidth || height > maxHeight) {
          this.scaleRatio = Math.min(maxWidth / width, maxHeight / height);
          width = width * this.scaleRatio;
          height = height * this.scaleRatio;
        } else {
          this.scaleRatio = 1;
        }
        canvas.width = width;
        canvas.height = height;
        this.drawImage();
      }
    };
    this.img.onerror = (error) => {
      console.error('Error loading image:', error);
    };
    this.img.src = src;
  }
  
  private drawImage() {
    if (!this.ctx || !this.canvasRef || !this.img) {
      console.error('drawImage failed - missing:', {
        ctx: !!this.ctx,
        canvas: !!this.canvasRef,
        img: !!this.img
      });
      return;
    }
    
    const canvas = this.canvasRef.nativeElement;
    this.ctx.clearRect(0, 0, canvas.width, canvas.height);
    this.ctx.drawImage(this.img, 0, 0, canvas.width, canvas.height);
    if (this.cropArea) {
      this.drawCropOverlay();
    }
  }
  
  private drawCropOverlay() {
    if (!this.cropArea || !this.ctx || !this.canvasRef || !this.img) return;
    const { x, y, width, height } = this.cropArea;
    const canvas = this.canvasRef.nativeElement;
    this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    this.ctx.fillRect(0, 0, canvas.width, canvas.height);
    this.ctx.clearRect(x, y, width, height);
    this.ctx.drawImage(this.img, 0, 0, canvas.width, canvas.height);
    this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    this.ctx.fillRect(0, 0, canvas.width, y); // Top
    this.ctx.fillRect(0, y, x, height); // Left
    this.ctx.fillRect(x + width, y, canvas.width - (x + width), height); // Right
    this.ctx.fillRect(0, y + height, canvas.width, canvas.height - (y + height)); // Bottom
    this.ctx.strokeStyle = '#00ff00';
    this.ctx.lineWidth = 3;
    this.ctx.strokeRect(x, y, width, height);
    this.drawHandle(x, y);
    this.drawHandle(x + width, y);
    this.drawHandle(x, y + height);
    this.drawHandle(x + width, y + height);
  }
  
  private drawHandle(x: number, y: number) {
    if (!this.ctx) return;
    this.ctx.fillStyle = '#00ff00';
    this.ctx.fillRect(x - 4, y - 4, 8, 8);
    this.ctx.strokeStyle = '#000';
    this.ctx.lineWidth = 1;
    this.ctx.strokeRect(x - 4, y - 4, 8, 8);
  }
  
  onMouseDown(event: MouseEvent) {
    if (!this.canvasRef) return;
    const canvas = this.canvasRef.nativeElement;
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    this.startX = (event.clientX - rect.left) * scaleX;
    this.startY = (event.clientY - rect.top) * scaleY;
    this.isDrawing = true;
  }
  
  onMouseMove(event: MouseEvent) {
    if (!this.isDrawing || !this.canvasRef) return;
    if (!this.ctx && this.canvasRef) {
      this.ctx = this.canvasRef.nativeElement.getContext('2d');
    }
    const canvas = this.canvasRef.nativeElement;
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    const currentX = (event.clientX - rect.left) * scaleX;
    const currentY = (event.clientY - rect.top) * scaleY;
    const x = Math.min(this.startX, currentX);
    const y = Math.min(this.startY, currentY);
    const width = Math.abs(currentX - this.startX);
    const height = Math.abs(currentY - this.startY);
    this.cropArea = { 
      x: Math.round(x), 
      y: Math.round(y), 
      width: Math.round(width), 
      height: Math.round(height) 
    };
    this.drawImage();
  }
  
  onMouseUp(event: MouseEvent) {
    this.isDrawing = false;
    if (this.cropArea && this.img) {
      const originalCropArea = {
        x: Math.round(this.cropArea.x / this.scaleRatio),
        y: Math.round(this.cropArea.y / this.scaleRatio),
        width: Math.round(this.cropArea.width / this.scaleRatio),
        height: Math.round(this.cropArea.height / this.scaleRatio)
      };
      this.cropSelected.emit(originalCropArea);
    }
  }
  
  resetCrop() {
    this.cropArea = null;
    this.drawImage();
  }
  
  applyCrop() {
    if (this.cropArea && this.img) {
      const originalCropArea = {
        x: Math.round(this.cropArea.x / this.scaleRatio),
        y: Math.round(this.cropArea.y / this.scaleRatio),
        width: Math.round(this.cropArea.width / this.scaleRatio),
        height: Math.round(this.cropArea.height / this.scaleRatio)
      };
      this.cropSelected.emit(originalCropArea);
    }
  }
  
  getCropData(): CropArea | null {
    return this.cropArea;
  }
  
  forceRedraw() {
    if (this.ctx && this.canvasRef && this.img) {
      const canvas = this.canvasRef.nativeElement;
      this.ctx.fillStyle = 'red';
      this.ctx.fillRect(0, 0, 100, 100);
      this.ctx.drawImage(this.img, 0, 0, canvas.width, canvas.height);
    } else {
      console.error('Missing components:', {
        ctx: !!this.ctx,
        canvas: !!this.canvasRef,
        img: !!this.img
      });
    }
  }
}