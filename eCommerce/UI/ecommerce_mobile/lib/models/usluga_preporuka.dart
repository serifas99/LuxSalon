import 'package:json_annotation/json_annotation.dart';

import 'usluga.dart';

part 'usluga_preporuka.g.dart';

@JsonSerializable()
class UslugaPreporuka {
  final Usluga? usluga;
  final double? skor;
  final double? contentBasedSkor;
  final double? popularityBasedSkor;
  final String? objasnjenje;

  UslugaPreporuka({
    this.usluga,
    this.skor,
    this.contentBasedSkor,
    this.popularityBasedSkor,
    this.objasnjenje,
  });

  factory UslugaPreporuka.fromJson(Map<String, dynamic> json) =>
      _$UslugaPreporukaFromJson(json);

  Map<String, dynamic> toJson() => _$UslugaPreporukaToJson(this);
}
