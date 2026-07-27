import 'package:ecommerce_desktop/providers/base_provider.dart';

/// Minimalan provider za Placanje - desktop (Admin/Frizer) trenutno treba samo
/// "Vrati novac" (refund) akciju za zavrseno placanje; ne treba puni CRUD/model
/// kao za ostale entitete.
class PlacanjeProvider extends BaseProvider<dynamic> {
  PlacanjeProvider() : super("Placanje");

  @override
  dynamic fromJson(data) => data;

  /// POST Placanje/{id}/Vrati - vraca novac klijentu (PayPal refund) i postavlja
  /// status placanja na "Vraceno". Backend dozvoljava samo Admin/Frizer (ownership
  /// provjera u PlacanjeService.ProvjeriOvlascenjeZaRefund).
  Future<dynamic> vrati(int placanjeId) => customAction(placanjeId, "Vrati");
}
