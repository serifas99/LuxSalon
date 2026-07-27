import 'package:ecommerce_desktop/providers/base_provider.dart';

import '../models/radno_vrijeme.dart';

class RadnoVrijemeProvider extends BaseProvider<RadnoVrijeme> {
  RadnoVrijemeProvider() : super("RadnoVrijeme");

  @override
  RadnoVrijeme fromJson(data) {
    return RadnoVrijeme.fromJson(data);
  }
}
