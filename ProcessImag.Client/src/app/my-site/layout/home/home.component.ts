import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HomeService, Subscription } from './home.service';
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
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})

export class HomeComponent implements OnInit {
  subscriptions: Subscription[] = [];
  loading = true;
  error = '';
  selectedPlanId: number | null = null;

  private apiUrl = 'https://localhost:7001/api/subscriptions';
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


  constructor(private homeService: HomeService) { }

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
        console.log('Abonamente încărcate:', data);
      },
      error: (err) => {
        this.error = 'Eroare la încărcarea abonamentelor. Vă rugăm încercați din nou.';
        this.loading = false;
        console.error('Eroare API:', err);
      }
    });
  }




  getPlanConfig(tip: string): PlanConfig {
    return this.planConfigs[tip] || this.planConfigs['Free'];
  }

  getFeatures(subscription: Subscription): string[] {
    const features: string[] = [];

    // Adaugă stocare
    features.push(`${this.formatStorage(subscription.dimensiuneMaximaMb)} stocare maximă`);

    // Adaugă tipurile de procesare cu limitele lor
    if (subscription.subscripteProcesares && subscription.subscripteProcesares.length > 0) {
      // Sortează după ID-ul tipului de procesare pentru a fi consistent
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

    // Adaugă features generale bazate pe tip
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
    this.homeService.activateSubscription(subscription.id).subscribe({
      next: (response) => {
        alert(response.message);
        this.selectedPlanId = null;
      },
      error: (err) => {
        alert('Eroare la activarea abonamentului. Vă rugăm încercați din nou.');
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

  getProcessingCount(subscription: Subscription): number {
    return subscription.subscripteProcesares?.length || 0;
  }
}