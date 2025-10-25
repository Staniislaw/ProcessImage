import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface TipProcesare {
    id: number;
    nume: string;
}

export interface SubscriptieProcesare {
    id: number;
    subscriptieId: number;
    tipProcesareId: number;
    limitaMax: number | null;
    subscriptie: any;
    tipProcesare: TipProcesare;
}

export interface Subscription {
    id: number;
    tip: string;
    pret: number;
    dimensiuneMaximaMb: number;
    subscriptieProcesareId: number;
    subscripteProcesares: SubscriptieProcesare[];
    utilizators: any[];
}

export interface SubscriptionLimits {
    subscriptieId: number;
    tip: string;
    limite: {
        tipProcesare: string;
        tipProcesareId: number;
        limitaMax: number | null;
        esteLimitat: boolean;
        descriere: string;
    }[];
}

export interface SubscriptionReport {
    id: number;
    tip: string;
    pret: number;
    dimensiuneMaximaMb: number;
    subscriptieProcesareId: number;
    numarTipuriProcesare: number;
    tipuriProcesare: {
        id: number;
        nume: string;
        tipProcesareId: number;
        limitaMax: number | null;
        esteLimitat: boolean;
        status: string;
    }[];
}

@Injectable({
    providedIn: 'root'
})
export class HomeService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiBaseUrl + '/Subscriptions';
    getAllSubscriptions(): Observable<Subscription[]> {
        return this.http.get<Subscription[]>(this.apiUrl);
    }
    getSubscriptionById(id: number): Observable<Subscription> {
        return this.http.get<Subscription>(`${this.apiUrl}/${id}`);
    }
    getSubscriptionByType(tip: string): Observable<Subscription> {
        return this.http.get<Subscription>(`${this.apiUrl}/type/${tip}`);
    }
    getAllProcessingTypes(): Observable<TipProcesare[]> {
        return this.http.get<TipProcesare[]>(`${this.apiUrl}/processing-types`);
    }
    getSubscriptionLimits(id: number): Observable<SubscriptionLimits> {
        return this.http.get<SubscriptionLimits>(`${this.apiUrl}/${id}/limits`);
    }
    checkProcessingLimit(utilizatorId: number, tipProcesareId: number): Observable<any> {
        return this.http.get(`${this.apiUrl}/check-limit`, {
            params: {
                utilizatorId: utilizatorId.toString(),
                tipProcesareId: tipProcesareId.toString()
            }
        });
    }
    getSubscriptionsReport(): Observable<SubscriptionReport[]> {
        return this.http.get<SubscriptionReport[]>(`${this.apiUrl}/report`);
    }
    activateSubscription(subscriptionId: number): Observable<any> {
        return this.http.post(`${this.apiUrl}/activate/${subscriptionId}`, {});
    }

}