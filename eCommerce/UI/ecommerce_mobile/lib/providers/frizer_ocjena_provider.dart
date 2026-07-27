import 'dart:convert';

import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:http/http.dart' as http;

import '../models/frizer_ocjena.dart';

class FrizerOcjenaProvider extends BaseProvider<FrizerOcjena> {
  FrizerOcjenaProvider() : super("FrizerOcjena");

  @override
  FrizerOcjena fromJson(data) {
    return FrizerOcjena.fromJson(data);
  }

  /// Prosjecna ocjena frizera (0 ako jos nema ocjena) - GET FrizerOcjena/ProsjecnaOcjena/{frizerId}.
  Future<double> prosjecnaOcjena(int frizerId) async {
    var url = "${BaseProvider.baseUrl}$endpoint/ProsjecnaOcjena/$frizerId";
    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.get(uri, headers: headers);

    validateResponse(response);
    var data = jsonDecode(response.body);
    return (data as num).toDouble();
  }
}
