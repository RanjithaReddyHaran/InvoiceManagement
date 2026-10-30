import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Client, InvoiceApiService, InvoiceSummary } from './invoice-api.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  invoices: InvoiceSummary[] = [];
  clients: Client[] = [];
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';
  form = { clientId: 0, periodStart: '', periodEnd: '', taxRate: 0, notes: '' };

  constructor(private readonly api: InvoiceApiService) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.errorMessage = '';
    this.api.getClients().subscribe({
      next: clients => { this.clients = clients; },
      error: error => this.handleError(error)
    });
    this.api.getInvoices().subscribe({
      next: invoices => { this.invoices = invoices; this.loading = false; },
      error: error => { this.loading = false; this.handleError(error); }
    });
  }

  createInvoice(): void {
    this.errorMessage = '';
    this.successMessage = '';
    if (!this.form.clientId || !this.form.periodStart || !this.form.periodEnd) {
      this.errorMessage = 'Select a client and billing period before generating an invoice.';
      return;
    }
    if (this.form.periodEnd < this.form.periodStart) {
      this.errorMessage = 'Period end must be on or after period start.';
      return;
    }
    this.saving = true;
    this.api.createInvoice(this.form).subscribe({
      next: result => {
        this.successMessage = `Invoice ${result.invoiceNumber} was generated successfully.`;
        this.form = { clientId: 0, periodStart: '', periodEnd: '', taxRate: 0, notes: '' };
        this.saving = false;
        this.loadData();
      },
      error: error => { this.saving = false; this.handleError(error); }
    });
  }

  approve(id: number): void {
    this.api.approveInvoice(id).subscribe({
      next: () => { this.successMessage = 'Invoice approved.'; this.loadData(); },
      error: error => this.handleError(error)
    });
  }

  downloadPdf(invoice: InvoiceSummary): void {
    this.api.getPdf(invoice.id).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `${invoice.invoiceNumber}.pdf`;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: error => this.handleError(error)
    });
  }

  get totalBilled(): number { return this.invoices.reduce((sum, item) => sum + item.totalAmount, 0); }
  get totalPaid(): number { return this.invoices.reduce((sum, item) => sum + item.amountPaid, 0); }
  get outstanding(): number { return this.invoices.reduce((sum, item) => sum + item.balanceDue, 0); }

  private handleError(error: HttpErrorResponse): void {
    this.errorMessage = error.error?.detail || error.error?.message || 'Unable to complete the request. Check the API connection and try again.';
  }
}
