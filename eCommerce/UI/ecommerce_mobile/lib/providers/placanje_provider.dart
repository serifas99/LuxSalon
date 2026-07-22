import 'dart:convert';

import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:http/http.dart' as http;

import '../models/placanje.dart';

/// Placanje nema generickу listu (get()) na klijentu, samo posebne akcije,
/// pa ne koristimo standardni BaseProvider<T>.get() nego direktne pozive.
class PlacanjeProvider extends BaseProvider<Placanje> {
  PlacanjeProvider() : super("Placanje");

  @override
  Placanje fromJson(data) {
    return Placanje.fromJson(data);
  }

  /// Kreira PayPal narudzbu za dati termin. Vraca link na koji treba
  /// preusmjeriti korisnika da odobri placanje (sandbox).
  Future<PlacanjeKreirajResponse> kreiraj(int terminId) async {
    var url = "${BaseProvider.baseUrl}$endpoint/Kreiraj/$terminId";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.post(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body);
    return PlacanjeKreirajResponse.fromJson(data);
  }

  /// Potvrdjuje (kaptira) placanje nakon sto je korisnik odobrio na PayPal stranici.
  Future<Placanje> potvrdi(String paypalOrderId) async {
    var url = "${BaseProvider.baseUrl}$endpoint/Potvrdi/$paypalOrderId";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.post(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body);
    return fromJson(data);
  }
}
