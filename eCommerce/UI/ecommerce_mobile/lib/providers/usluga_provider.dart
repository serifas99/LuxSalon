import 'package:ecommerce_mobile/providers/base_provider.dart';

import '../models/usluga.dart';

class UslugaProvider extends BaseProvider<Usluga> {
  UslugaProvider() : super("Usluga");

  @override
  Usluga fromJson(data) {
    return Usluga.fromJson(data);
  }
}
