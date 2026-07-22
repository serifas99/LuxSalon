import 'package:ecommerce_desktop/providers/base_provider.dart';

import '../models/termin.dart';

class TerminProvider extends BaseProvider<Termin> {
  TerminProvider() : super("Termin");

  @override
  Termin fromJson(data) {
    return Termin.fromJson(data);
  }

  Future<Termin> potvrdi(int id) => customAction(id, "Potvrdi");

  Future<Termin> otkazi(int id) => customAction(id, "Otkazi");

  Future<Termin> oznaciOdradjen(int id) => customAction(id, "OznaciOdradjen");

  Future<Termin> oznaciNijeSeOdazvao(int id) =>
      customAction(id, "OznaciNijeSeOdazvao");
}
