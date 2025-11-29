import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { HomeService, Subscription } from '../../my-site/layout/home/home.service';

interface PlanConfig {
  description: string;
  gradient: string;
  border: string;
  buttonColor: string;
  badgeColor: string;
  icon: string;
  popular?: boolean;
}

@Component({
  selector: 'app-upgrade-plan-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './upgrade-plan-dialog.component.html',
  styleUrl: './upgrade-plan-dialog.component.css'
})
export class UpgradePlanDialogComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<UpgradePlanDialogComponent>);
  private homeService = inject(HomeService);
  private toastr = inject(ToastrService);

  subscriptions: Subscription[] = [];
  loading = true;
  error = '';
  selectedPlanId: number | null = null;
  isProcessing = false;

  planConfigs: { [key: string]: PlanConfig } = {
    'Free': {
      description: 'Perfect pentru început',
      gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      border: 'border-purple-400',
      buttonColor: 'bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700',
      badgeColor: 'bg-purple-100 text-purple-800',
      icon: '🚀'
    },
    'Premium': {
      description: 'Pentru utilizatori avansați',
      gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      border: 'border-pink-400',
      buttonColor: 'bg-gradient-to-r from-pink-600 to-rose-600 hover:from-pink-700 hover:to-rose-700',
      badgeColor: 'bg-pink-100 text-pink-800',
      icon: '⭐',
      popular: true
    },
    'Professional': {
      description: 'Pentru profesioniști și echipe',
      gradient: 'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)',
      border: 'border-amber-400',
      buttonColor: 'bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700',
      badgeColor: 'bg-amber-100 text-amber-800',
      icon: '👑'
    }
  };

  ngOnInit(): void {
    this.loadSubscriptions();
  }

  loadSubscriptions(): void {
    this.loading = true;
    this.error = '';

    this.homeService.getAllSubscriptions().subscribe({
      next: (data) => {
        this.subscriptions = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Eroare la încărcarea abonamentelor. Vă rugăm încercați din nou.';
        this.loading = false;
        this.toastr.error('Eroare API', 'Eroare');
      }
    });
  }

  getPlanConfig(tip: string): PlanConfig {
    return this.planConfigs[tip] || this.planConfigs['Free'];
  }

  getFeatures(subscription: Subscription): string[] {
    const features: string[] = [];
    features.push(`${this.formatStorage(subscription.dimensiuneMaximaMb)} stocare maximă`);
    
    if (subscription.subscripteProcesares && subscription.subscripteProcesares.length > 0) {
      const sortedProcessing = [...subscription.subscripteProcesares].sort(
        (a, b) => a.tipProcesare.id - b.tipProcesare.id
      );
      sortedProcessing.forEach(sp => {
        if (sp.limitaMax === null) {
          features.push(`✓ ${sp.tipProcesare.nume} - Nelimitat`);
        } else {
          features.push(`✓ ${sp.tipProcesare.nume} - ${sp.limitaMax}/zi`);
        }
      });
    }

    if (subscription.tip === 'Free') {
      features.push('Suport comunitate');
    } else if (subscription.tip === 'Premium') {
      features.push('Suport prioritar 24/7');
      features.push('Procesare mai rapidă');
    } else if (subscription.tip === 'Professional') {
      features.push('Suport dedicat 24/7');
      features.push('Procesare prioritară');
      features.push('API acces complet');
    }

    return features;
  }

  getProcessingSummary(subscription: Subscription): string {
    if (!subscription.subscripteProcesares || subscription.subscripteProcesares.length === 0) {
      return 'Fără opțiuni de procesare';
    }

    const unlimited = subscription.subscripteProcesares.filter(sp => sp.limitaMax === null).length;
    const limited = subscription.subscripteProcesares.filter(sp => sp.limitaMax !== null).length;

    if (unlimited === subscription.subscripteProcesares.length) {
      return `${unlimited} tipuri de procesare nelimitate`;
    } else if (limited === subscription.subscripteProcesares.length) {
      return `${limited} tipuri de procesare cu limită zilnică`;
    } else {
      return `${unlimited} nelimitate + ${limited} cu limită`;
    }
  }

  hasUnlimitedProcessing(subscription: Subscription): boolean {
    if (!subscription.subscripteProcesares || subscription.subscripteProcesares.length === 0) {
      return false;
    }
    return subscription.subscripteProcesares.every(sp => sp.limitaMax === null);
  }

  selectPlan(subscription: Subscription): void {
    this.selectedPlanId = subscription.id;
    this.isProcessing = true;

    this.homeService.activateSubscription(subscription.id).subscribe({
      next: (response) => {
        this.toastr.success('Abonament activat cu succes!', 'Succes');
        this.isProcessing = false;
        // Returnează true pentru a semnala upgrade-ul
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.toastr.error('Eroare la activarea abonamentului. Vă rugăm încercați din nou', 'Eroare');
        this.isProcessing = false;
        this.selectedPlanId = null;
      }
    });
  }

  isPopular(tip: string): boolean {
    return this.planConfigs[tip]?.popular || false;
  }

  formatPrice(price: number): string {
    return price.toFixed(2);
  }

  formatStorage(mb: number): string {
    if (mb >= 1000) {
      return `${(mb / 1000).toFixed(1)} GB`;
    }
    return `${mb} MB`;
  }

  closeDialog(): void {
    this.dialogRef.close(false);
  }
}