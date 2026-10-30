import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Client { id: number; name: string; billingEmail: string; }
export interface InvoiceSummary {
  id: number; invoiceNumber: string; clientName: string; invoiceDate: string;
  dueDate: string; totalAmount: number; amountPaid: number; balanceDue: number; status: string;
}

@Injectable({ providedIn: 'root' })
export class InvoiceApiService {
  private readonly baseUrl = 'https://localhost:7181/api';
  constructor(private readonly http: HttpClient) { }
  getInvoices(): Observable<InvoiceSummary[]> { return this.http.get<InvoiceSummary[]>(`${this.baseUrl}/invoices`); }
  getClients(): Observable<Client[]> { return this.http.get<Client[]>(`${this.baseUrl}/clients`); }
  createInvoice(request: { clientId: number; periodStart: string; periodEnd: string; taxRate: number; notes: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/invoices`, request);
  }
  approveInvoice(id: number): Observable<any> { return this.http.post(`${this.baseUrl}/invoices/${id}/approve`, {}); }
  getPdf(id: number): Observable<Blob> { return this.http.get(`${this.baseUrl}/invoices/${id}/pdf`, { responseType: 'blob' }); }
}
