import 'dart:convert';

import 'package:ecommerce_mobile/providers/base_provider.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

import '../models/usluga_preporuka.dart';

/// Poziva RecommendationController koji vraca "golu" listu (nije SearchResult),
/// pa ne nasljedjuje BaseProvider<T>.
class RecommendationProvider with ChangeNotifier {
  Future<List<UslugaPreporuka>> preporuke(int klijentId, {int broj = 5}) async {
    var url = "${BaseProvider.baseUrl}Recommendation/$klijentId?broj=$broj";
    var uri = Uri.parse(url);

    var response = await http.get(uri);

    if (response.statusCode >= 300) {
      throw Exception("Greška prilikom dohvatanja preporuka");
    }

    var data = jsonDecode(response.body) as List<dynamic>;
    return data
        .map((e) => UslugaPreporuka.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
