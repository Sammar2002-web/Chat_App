import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class TestService {
  private hubConnection!: signalR.HubConnection;

  public async startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7086/testHub')
      .build();

    await this.hubConnection.start();
  }

  public async echo(message: string): Promise<string> {
    return await this.hubConnection.invoke('Echo', message);
  }
}
