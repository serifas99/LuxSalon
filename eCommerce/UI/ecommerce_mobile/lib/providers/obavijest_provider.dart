import 'package:ecommerce_mobile/providers/base_provider.dart';

import '../models/obavijest.dart';

class ObavijestProvider extends BaseProvider<Obavijest> {
  ObavijestProvider() : super("Obavijest");

  @override
  Obavijest fromJson(data) {
    return Obavijest.fromJson(data);
  }
}
