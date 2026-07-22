import 'package:ecommerce_desktop/providers/base_provider.dart';

import '../models/usluga_kategorija.dart';

class UslugaKategorijaProvider extends BaseProvider<UslugaKategorija> {
  UslugaKategorijaProvider() : super("UslugaKategorija");

  @override
  UslugaKategorija fromJson(data) {
    return UslugaKategorija.fromJson(data);
  }
}
