import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:jwt_decoder/jwt_decoder.dart';

import '../utils/api_client_exception.dart';

class AuthProvider extends ChangeNotifier {
  bool _isAuthenticated = false;
  static String? _accesstoken;
  String? _refreshtoken;
  static Map<String, dynamic>? _accessTokenDecoded;

  static String? get accesstoken => _accesstoken;
  String? get refreshtoken => _refreshtoken;
  static Map<String, dynamic>? get accessTokenDecoded => _accessTokenDecoded;

  String _baseUrl = "";

  AuthProvider() {
    // Ista "baseUrl" --dart-define varijabla kao BaseProvider/SignalRService - ranije je ovdje
    // bio drugaciji kljuc ("BASE_URL"), pa promjena baseUrl-a pri pokretanju nije uticala na login.
    _baseUrl = const String.fromEnvironment("baseUrl", defaultValue: "http://localhost:5126/");
  }



  bool get isAuthenticated => _isAuthenticated;

  Future login(String username, String password) async {
    var url = "${_baseUrl}Access/login";
    print("Login url: $url");
    var uri = Uri.parse(url);
    var headers = createHeaders();
    //todo: refactor this into a proper class. Not a good practice but for sample purposes it's ok
    var body = jsonEncode({
      "username": username,
      "password": password
    });

    http.Response response = await http.post(uri, headers: headers, body: body);

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      _accesstoken = data['accesstoken'];
      _refreshtoken = data['refreshtoken'];
      _isAuthenticated = true;
      _accessTokenDecoded = JwtDecoder.decode(_accesstoken ?? "");
      notifyListeners();
    } else {
      throw Exception("Unknown error");
    }

  }


    bool isValidResponse(http.Response response) {
    if (response.statusCode < 299) {
      return true;
    }

    // Pokusaj izvuci pravu poruku sa servera (npr. "Pogresno korisnicko ime ili lozinka")
    // umjesto generickog teksta - isti parser koji koristi BaseProvider za ostale pozive.
    final parsed = ApiErrorParser.messageFromBody(response.body);
    if (response.statusCode == 401) {
      throw ApiClientException(parsed ?? "Pogrešno korisničko ime ili lozinka.");
    }
    throw ApiClientException(parsed ?? "Prijava nije uspjela. Pokušajte ponovo.");
  }

  void logout() {
    _isAuthenticated = false;
    _accesstoken = null;
    _refreshtoken = null;
    _accessTokenDecoded = null;
    notifyListeners();
  }


  Map<String, String> createHeaders() {

    var headers = {
      "Content-Type": "application/json",
    };

    return headers;
  }
  
}