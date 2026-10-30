import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = 'https://localhost:7181/api';
  constructor(private readonly http: HttpClient) {}

  getInvoices(): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}/invoices`); }
  getClients(): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}/clients`); }
  createInvoice(payload: { clientId: number; periodStart: string; periodEnd: string; taxRate: number; notes?: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/invoices`, payload);
  }
  approveInvoice(id: number): Observable<any> { return this.http.post(`${this.baseUrl}/invoices/${id}/approve`, {}); }
  downloadInvoicePdf(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/invoices/${id}/pdf`, { responseType: 'blob' });
  }
}
