import 'dart:convert';

import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:http/http.dart' as http;

import '../models/dostupnost_dana.dart';
import '../models/termin.dart';

class TerminProvider extends BaseProvider<Termin> {
  TerminProvider() : super("Termin");

  @override
  Termin fromJson(data) {
    return Termin.fromJson(data);
  }

  Future<Termin> _customAction(int id, String action) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/$action";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.post(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body);
    return fromJson(data);
  }

  Future<Termin> otkazi(int id) => _customAction(id, "Otkazi");

  /// Dostupnost svakog dana u zadanom mjesecu za frizera/uslugu - za bojenje
  /// color-coded kalendara (zeleno/crveno).
  Future<List<DostupnostDana>> dostupnost({
    required int frizerId,
    required int uslugaId,
    required int godina,
    required int mjesec,
  }) async {
    var url =
        "${BaseProvider.baseUrl}$endpoint/Dostupnost?frizerId=$frizerId&uslugaId=$uslugaId&godina=$godina&mjesec=$mjesec";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.get(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body) as List;
    return data.map((e) => DostupnostDana.fromJson(e)).toList();
  }

  /// Konkretni slobodni vremenski slotovi ("HH:mm") za odabrani dan/frizera/uslugu.
  Future<List<String>> dostupniSlotovi({
    required int frizerId,
    required int uslugaId,
    required DateTime datum,
  }) async {
    var datumStr =
        "${datum.year.toString().padLeft(4, '0')}-${datum.month.toString().padLeft(2, '0')}-${datum.day.toString().padLeft(2, '0')}";
    var url =
        "${BaseProvider.baseUrl}$endpoint/DostupniSlotovi?frizerId=$frizerId&uslugaId=$uslugaId&datum=$datumStr";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.get(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body) as List;
    return data.map((e) => e.toString()).toList();
  }
}
