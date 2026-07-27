import 'dart:convert';

import 'package:ecommerce_desktop/providers/base_provider.dart';
import 'package:http/http.dart' as http;

import '../models/klijent_pregled.dart';
import '../models/search_result.dart';
import '../models/user.dart';

class UserProvider extends BaseProvider<User> {
  UserProvider() : super("Users");

  @override
  User fromJson(data) {
    return User.fromJson(data);
  }

  /// Pregled klijenata (Users/Klijenti) - za desktop "Klijenti" ekran, vidi klijent_list.dart.
  Future<SearchResult<KlijentPregled>> klijenti({dynamic filter}) async {
    var url = "${BaseProvider.baseUrl}Users/Klijenti";
    if (filter != null) {
      var queryString = getQueryString(filter);
      url = "$url?$queryString";
    }

    var uri = Uri.parse(url);
    var headers = createHeaders();

    var response = await http.get(uri, headers: headers);
    validateResponse(response);

    var data = jsonDecode(response.body);
    var result = SearchResult<KlijentPregled>();
    result.totalCount = data['totalCount'];
    result.items = List<KlijentPregled>.from(
        data["items"].map((e) => KlijentPregled.fromJson(e)));

    return result;
  }
}
