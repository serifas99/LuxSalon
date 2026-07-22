import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:flutter/material.dart';
import 'package:signalr_netcore/signalr_client.dart';

/// Wrapper oko SignalR konekcije za primanje notifikacija uzivo (bez ručnog refresha).
class SignalRService {
  SignalRService._();
  static final SignalRService instance = SignalRService._();

  HubConnection? _hubConnection;

  /// Poziva se svaki put kad stigne nova notifikacija sa servera.
  void Function(Map<String, dynamic> notifikacija)? onNotifikacija;

  bool get isConnected =>
      _hubConnection?.state == HubConnectionState.Connected;

  Future<void> connect() async {
    if (isConnected) return;

    // Cita istu --dart-define vrijednost kao BaseProvider ("baseUrl"), ali nezavisno od
    // njega - BaseProvider._baseUrl se postavlja tek kad se prvi put upotrijebi neki
    // provider (lazy), a SignalR se konektuje odmah nakon logina, prije toga.
    const baseUrl = String.fromEnvironment(
      "baseUrl",
      defaultValue: "http://10.0.2.2:5126/",
    );
    final hubUrl = "${baseUrl}hubs/notifikacije";

    _hubConnection = HubConnectionBuilder()
        .withUrl(
          hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => AuthProvider.accesstoken ?? "",
            transport: HttpTransportType.WebSockets,
          ),
        )
        .withAutomaticReconnect()
        .build();

    _hubConnection!.on("NovaNotifikacija", (arguments) {
      if (arguments == null || arguments.isEmpty) return;
      final data = arguments[0];
      if (data is Map) {
        onNotifikacija?.call(Map<String, dynamic>.from(data));
      }
    });

    try {
      await _hubConnection!.start();
      debugPrint("SignalR: konekcija uspostavljena ($hubUrl)");
    } catch (e) {
      debugPrint("SignalR: greska pri konektovanju - $e");
    }
  }

  Future<void> disconnect() async {
    try {
      await _hubConnection?.stop();
    } catch (_) {}
    _hubConnection = null;
  }
}
