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
  
  ngAfterViewInit() {
    const canvas = this.canvasRef?.nativeElement;
    if (canvas) {
      this.ctx = canvas.getContext('2d');
      console.log('Canvas initialized:', canvas);
    }
  }
  
  loadImage(src: string) {
    console.log('loadImage called with:', src?.substring(0, 50));
    this.imageSrc = src;
    
    // IMPORTANT: Reinițializăm contextul aici!
    if (this.canvasRef) {
      this.ctx = this.canvasRef.nativeElement.getContext('2d');
      console.log('Context reinitialized:', !!this.ctx);
    }
    
    this.img = new Image();
    this.img.crossOrigin = 'anonymous';
    this.img.onload = () => {
      console.log('Image loaded successfully:', this.img?.width, 'x', this.img?.height);
      if (this.canvasRef && this.img) {
        const canvas = this.canvasRef.nativeElement;
        
        // Asigurăm că avem contextul
        if (!this.ctx) {
          this.ctx = canvas.getContext('2d');
          console.log('Context created in onload:', !!this.ctx);
        }
        
        // Scalează imaginea dacă este prea mare
        const maxWidth = 800;
        const maxHeight = 600;
        let width = this.img.width;
        let height = this.img.height;
        
        if (width > maxWidth || height > maxHeight) {
          const ratio = Math.min(maxWidth / width, maxHeight / height);
          width = width * ratio;
          height = height * ratio;
        }
        
        canvas.width = width;
        canvas.height = height;
        console.log('Canvas size set to:', canvas.width, 'x', canvas.height);
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
    console.log('Drawing image on canvas:', canvas.width, 'x', canvas.height);
    
    // Clear canvas
    this.ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Desenează imaginea scalată la dimensiunea canvas-ului
    this.ctx.drawImage(this.img, 0, 0, canvas.width, canvas.height);
    console.log('Image drawn successfully');
    
    // Dacă există o zonă de crop, desenează overlay-ul și selecția
    if (this.cropArea) {
      console.log('Drawing crop overlay:', this.cropArea);
      this.drawCropOverlay();
    }
  }
  
  private drawCropOverlay() {
    if (!this.cropArea || !this.ctx || !this.canvasRef || !this.img) return;
    
    const { x, y, width, height } = this.cropArea;
    const canvas = this.canvasRef.nativeElement;
    
    // Desenează overlay întunecat peste toată imaginea
    this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    this.ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Șterge overlay-ul din zona selectată
    this.ctx.clearRect(x, y, width, height);
    
    // Redesenează porțiunea de imagine din zona selectată
    this.ctx.drawImage(this.img, 0, 0, canvas.width, canvas.height);
    
    // Aplicăm din nou overlay-ul, dar nu peste zona selectată
    this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    this.ctx.fillRect(0, 0, canvas.width, y); // Top
    this.ctx.fillRect(0, y, x, height); // Left
    this.ctx.fillRect(x + width, y, canvas.width - (x + width), height); // Right
    this.ctx.fillRect(0, y + height, canvas.width, canvas.height - (y + height)); // Bottom
    
    // Desenează conturul verde al selecției
    this.ctx.strokeStyle = '#00ff00';
    this.ctx.lineWidth = 3;
    this.ctx.strokeRect(x, y, width, height);
    
    // Desenează mânerele din colțuri
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
    
    // Asigură-te că avem contextul
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
    
    // Emite zona de crop când se termină selecția
    if (this.cropArea) {
      this.cropSelected.emit(this.cropArea);
    }
  }
  
  resetCrop() {
    this.cropArea = null;
    this.drawImage();
  }
  
  applyCrop() {
    if (this.cropArea) {
      this.cropSelected.emit(this.cropArea);
      console.log('Crop Area:', this.cropArea);
    }
  }
  
  getCropData(): CropArea | null {
    return this.cropArea;
  }
  
  forceRedraw() {
    console.log('Force redraw called');
    console.log('Canvas:', this.canvasRef?.nativeElement);
    console.log('Context:', this.ctx);
    console.log('Image:', this.img);
    
    if (this.ctx && this.canvasRef && this.img) {
      const canvas = this.canvasRef.nativeElement;
      console.log('Canvas dimensions:', canvas.width, 'x', canvas.height);
      console.log('Image dimensions:', this.img.width, 'x', this.img.height);
      
      // Test simplu - desenează un dreptunghi roșu
      this.ctx.fillStyle = 'red';
      this.ctx.fillRect(0, 0, 100, 100);
      console.log('Red rectangle drawn');
      
      // Apoi încearcă să desenezi imaginea
      this.ctx.drawImage(this.img, 0, 0, canvas.width, canvas.height);
      console.log('Image drawn');
    } else {
      console.error('Missing components:', {
        ctx: !!this.ctx,
        canvas: !!this.canvasRef,
        img: !!this.img
      });
    }
  }
}