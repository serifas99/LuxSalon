import 'package:ecommerce_mobile/providers/base_provider.dart';

import '../models/frizer.dart';

class FrizerProvider extends BaseProvider<Frizer> {
  FrizerProvider() : super("Frizer");

  @override
  Frizer fromJson(data) {
    return Frizer.fromJson(data);
  }
}
