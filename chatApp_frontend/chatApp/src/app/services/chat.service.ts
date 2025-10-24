import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';

export interface Message {
  id?: string;
  content: string;
  senderId: number;
  receiverId: number;
  timestamp?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = 'https://localhost:7086/api';
  private hubConnection!: signalR.HubConnection;
  private messageSubject = new Subject<any>();
  public connected = false;
  private connectionAttempts = 0;
  private maxConnectionAttempts = 5;

  constructor(private authService: AuthService, private http: HttpClient) {} 

  public async startConnection(): Promise<boolean> {
    const currentUser = this.authService.getDecodedToken();
    const currentUserId = currentUser?.UserId;

    if (!currentUserId) {
      console.warn('Cannot start SignalR connection — user not logged in.');
      return false;
    }

    if (this.connected && this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      console.log('Already connected to SignalR');
      return true;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7086/chatHub`, {
        accessTokenFactory: () => this.authService.getToken() || ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 15000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    try {
      console.log(`Starting SignalR connection for user: ${currentUserId}`);
      await this.hubConnection.start();
      this.connected = true;
      this.connectionAttempts = 0;
      console.log(`SignalR connected for userId: ${currentUserId}`);
      this.addReceiveMessageListener();
      this.addConnectionEventListeners();
      return true;
    } catch (err) {
      this.connectionAttempts++;
      console.error('SignalR connection error:', err);
      this.connected = false;
      
      if (this.connectionAttempts < this.maxConnectionAttempts) {
        console.log(`Retrying connection in 3 seconds... (Attempt ${this.connectionAttempts}/${this.maxConnectionAttempts})`);
        setTimeout(() => this.startConnection(), 3000);
      } else {
        console.error('Max connection attempts reached. Please refresh the page.');
      }
      return false;
    }
  }

  public addConnectionEventListeners() {
    this.hubConnection.onreconnected((connectionId) => {
      console.log('Reconnected to SignalR hub. Connection ID:', connectionId);
      this.connected = true;
      this.addReceiveMessageListener();
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('Reconnecting to SignalR hub due to:', error);
      this.connected = false;
    });

    this.hubConnection.onclose((error) => {
      console.warn('SignalR connection closed:', error);
      this.connected = false;
    });
  }

  public addReceiveMessageListener() {
    this.hubConnection.off('ReceiveMessage');
    
    this.hubConnection.on('ReceiveMessage', (data) => {
      console.log('Message received in service:', data);
      console.log('Current user should see this message if they are the receiver');
      this.messageSubject.next(data);
    });

    this.hubConnection.onclose((error) => {
      console.warn('SignalR connection closed:', error);
      this.connected = false;
    });
  }

  public getMessageObservable(): Observable<any> {
    return this.messageSubject.asObservable();
  }

  public async sendMessage(message: Message): Promise<any> {
    if (!message.receiverId) {
      throw new Error('ReceiverId is required');
    }

    if (!message.content?.trim()) {
      throw new Error('Message content is required');
    }

    console.log('Sending message to backend - From:', message.senderId, 'To:', message.receiverId);

    try {
      const result = await this.http.post(`${this.apiUrl}/Message/Create`, message).toPromise();
      console.log('Backend accepted message:', result);
      return result;
    } catch (error) {
      console.error('Failed to send message to backend:', error);
      throw error;
    }
  }

  public getConversation(userId1: string, userId2: string) {
    return this.http.get(`${this.apiUrl}/Message/conversation/${userId1}/${userId2}`);
  }

  public getConnectionState(): string {
    return this.hubConnection?.state || 'Disconnected';
  }

  public async reconnect(): Promise<boolean> {
    console.log('Manual reconnection requested');
    this.connectionAttempts = 0;
    return await this.startConnection();
  }

  public stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.connected = false;
      console.log('SignalR connection stopped');
    }
  }
}