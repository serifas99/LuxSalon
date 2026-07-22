import 'dart:convert';

import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:http/http.dart' as http;

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
}
