import 'dart:convert';

import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:http/http.dart' as http;

import '../models/notifikacija.dart';

class NotifikacijaProvider extends BaseProvider<Notifikacija> {
  NotifikacijaProvider() : super("Notifikacija");

  @override
  Notifikacija fromJson(data) {
    return Notifikacija.fromJson(data);
  }

  Future<Notifikacija> oznaciProcitano(int id) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/OznaciProcitano";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.post(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body);
    return fromJson(data);
  }
}
