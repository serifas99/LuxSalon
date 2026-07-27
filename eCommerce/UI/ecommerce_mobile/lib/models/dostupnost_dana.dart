import 'package:json_annotation/json_annotation.dart';

part 'dostupnost_dana.g.dart';

/// Dostupnost jednog dana za odabranog frizera/uslugu - koristi se za bojenje
/// color-coded kalendara (zeleno = ima slobodnih termina, crveno = nema, sivo = neradni dan).
@JsonSerializable()
class DostupnostDana {
  final DateTime datum;
  final bool radi;
  final bool slobodno;
  final int brojSlobodnihSlotova;

  DostupnostDana({
    required this.datum,
    required this.radi,
    required this.slobodno,
    required this.brojSlobodnihSlotova,
  });

  factory DostupnostDana.fromJson(Map<String, dynamic> json) =>
      _$DostupnostDanaFromJson(json);

  Map<String, dynamic> toJson() => _$DostupnostDanaToJson(this);
}
