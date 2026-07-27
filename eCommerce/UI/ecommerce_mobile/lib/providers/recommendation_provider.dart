import 'dart:convert';

import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

import '../models/usluga_preporuka.dart';

/// Poziva RecommendationController koji vraca "golu" listu (nije SearchResult),
/// pa ne nasljedjuje BaseProvider<T>. Endpoint zahtijeva JWT - klijentId se na backendu
/// izvodi iz tokena, ne prima se vise kao parametar (vidi RecommendationController.cs).
class RecommendationProvider with ChangeNotifier {
  Future<List<UslugaPreporuka>> preporuke({int broj = 5}) async {
    var url = "${BaseProvider.baseUrl}Recommendation?broj=$broj";
    var uri = Uri.parse(url);
    var headers = {
      "Content-Type": "application/json",
      "Authorization": "Bearer ${AuthProvider.accesstoken ?? ''}",
    };

    var response = await http.get(uri, headers: headers);

    if (response.statusCode >= 300) {
      throw Exception("Greška prilikom dohvatanja preporuka");
    }

    var data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((e) => UslugaPreporuka.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
